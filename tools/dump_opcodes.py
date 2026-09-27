#!/usr/bin/env python3
"""
Dump opcode (JamMessageID) tables from a decrypted Classic Era Wow binary.

Technique (works on name-stripped builds):
  - Every message class has a GetMsgId stub in .text:
        mov dword ptr [rX], <msgId> ; mov rax, rX ; retn
  - Each stub is referenced once from the message vtable in .rdata/.data
    (slot +0x18). Slots +0x08/+0x10 are the serialize fns: real code for
    outbound (CMSG) messages, both pointing at the shared "return false"
    dummy for inbound (SMSG) messages -> direction detection.
  - Whole opcode groups whose msgId is position-encoded (implicit index
    into per-group handler tables) are NOT visible this way; those are
    reported in the TSV gap report.

Usage:
    python dump_opcodes.py <wow.exe> <prev_version_dir_for_names> <out_dir> <version_name>

    python dump_opcodes.py "E:\\World of Warcraft\\_classic_era_\\WowClassic_decrypted.exe" ^
        V1_15_8_63829 V1_15_9_69722 Opcodes_1_15_9
"""
import struct, re, sys, os, collections

IMG_BASE = 0x140000000

def parse_pe(data):
    pe_off = struct.unpack_from('<I', data, 0x3C)[0]
    assert data[pe_off:pe_off+4] == b'PE\0\0', "not a PE"
    coff = pe_off + 4
    nsec = struct.unpack_from('<H', data, coff + 2)[0]
    size_opt = struct.unpack_from('<H', data, coff + 16)[0]
    sec_off = coff + 20 + size_opt
    secs = {}
    for i in range(nsec):
        o = sec_off + 40 * i
        name = data[o:o+8].rstrip(b'\0').decode('ascii', 'ignore')
        vsize, va, rsize, roff = struct.unpack_from('<IIII', data, o + 8)
        secs[name] = (va, vsize, roff, rsize)
    return secs

def main():
    exe, prev_ver, out_ver, cls_name = sys.argv[1:5]
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    data = open(exe, 'rb').read()
    secs = parse_pe(data)

    def va2off(va):
        rva = va - IMG_BASE
        for sva, vsize, roff, rsize in secs.values():
            if sva <= rva < sva + max(vsize, rsize):
                return roff + (rva - sva)
        return None
    def readq(va):
        o = va2off(va)
        return struct.unpack_from('<Q', data, o)[0] if o is not None else None

    tvx, tvs, tro, trs = secs['.text']
    tdata = data[tro:tro + trs]
    text_lo, text_hi = IMG_BASE + tvx, IMG_BASE + tvx + tvs

    # --- pass 1: GetMsgId stubs -------------------------------------------------
    stub_pat = re.compile(rb'\xC7[\x00-\x03](....)\x48[\x8B\x89][\xC0-\xD8]\xC3', re.S)
    stubs = {}
    for m in stub_pat.finditer(tdata):
        fva = text_lo + m.start()
        imm = struct.unpack('<I', m.group(1))[0]
        if 0x20 <= (imm >> 16) <= 0x7F and (imm & 0xFFFF) < 0x1000:
            stubs[fva] = imm
    stub_set = set(stubs)
    print(f"[+] GetMsgId stubs: {len(stubs)}")

    # --- pass 2: stub -> vtable -> direction ------------------------------------
    DUMMY_CAND = collections.Counter()
    ref_of_stub = {}
    for sec in ('.rdata', '.data', '_RDATA'):
        sva, vsize, roff, rsize = secs[sec]
        buf = data[roff:roff + rsize]
        lo = IMG_BASE + sva
        for i in range(0, len(buf) - 8, 8):
            v = struct.unpack_from('<Q', buf, i)[0]
            if v in stub_set:
                ref_of_stub[v] = lo + i

    # find the shared "return false" dummy used in slots1/2 of inbound vtables
    for stub, ref in ref_of_stub.items():
        s2 = readq(ref - 0x18 + 0x10)
        if s2: DUMMY_CAND[s2] += 1
    dummy = DUMMY_CAND.most_common(1)[0][0]
    print(f"[+] inbound dummy fn = {dummy:#x}")

    msgs = []  # (opcode, dir, stub_va, vtab_va, s1, s2)
    for stub, ref in ref_of_stub.items():
        vbase = ref - 0x18
        s1, s2 = readq(vbase + 8), readq(vbase + 16)
        direction = 'S' if (s1 == dummy and s2 == dummy) else 'C'
        msgs.append((stubs[stub], direction, stub, vbase, s1, s2))

    # --- known names ------------------------------------------------------------
    known = {}
    if prev_ver:
        p = os.path.join(root, 'WowPacketParser', 'Enums', 'Version', prev_ver, 'Opcodes.cs')
        t = open(p, encoding='utf8').read()
        for sec, d in (('ClientOpcodes', 'C'), ('ServerOpcodes', 'S'), ('MiscOpcodes', 'M')):
            m = re.search(sec + r'\s*=\s*new\(\)\s*\{(.*?)\};', t, re.S)
            if m:
                for mm in re.finditer(r'Opcode\.([A-Za-z0-9_]+)\s*,\s*0x([0-9A-Fa-f]+)', m.group(1)):
                    known[int(mm.group(2), 16)] = (mm.group(1), d)

    # --- write TSV --------------------------------------------------------------
    os.makedirs(os.path.join(root, 'tools'), exist_ok=True)
    tsv = os.path.join(root, 'tools', f'opcode_dump_{out_ver}.tsv')
    with open(tsv, 'w') as f:
        f.write("opcode\tdir\tname\tgetmsgid_va\tvtable_va\tslot1_va\tslot2_va\n")
        for opc, d, stub, vb, s1, s2 in sorted(msgs):
            nm = known.get(opc, ('',))[0]
            f.write(f"0x{opc:06X}\t{d}\t{nm}\t0x{stub:x}\t0x{vb:x}\t0x{s1:x}\t0x{s2:x}\n")
    print(f"[+] wrote {tsv} ({len(msgs)} rows)")

    # --- write Opcodes.cs -------------------------------------------------------
    outdir = os.path.join(root, 'WowPacketParser', 'Enums', 'Version', out_ver)
    os.makedirs(outdir, exist_ok=True)
    cdict = sorted((o, n) for (o, (n, d)) in known.items() if d in 'CM')
    sdict = sorted((o, n) for (o, (n, d)) in known.items() if d == 'S')
    cset = {o for o, *_ in cdict}; sset = {o for o, *_ in sdict}
    un_c = sorted(o for o, d, *_ in msgs if d == 'C' and o not in cset)
    un_s = sorted(o for o, d, *_ in msgs if d == 'S' and o not in sset)

    dumped_vals = {o for o, *_ in msgs}
    def block(items, unknown):
        out = []
        for o, n in items:
            note = " // sniff-verified, not in vtable dump" if o not in dumped_vals else ""
            out.append(f"            {{ Opcode.{n}, 0x{o:X} }},{note}")
        out.append("            // --- unnamed opcodes recovered from client binary (identify via serializer fns, see tools/opcode_dump_{0}.tsv)".format(out_ver))
        for o in unknown:
            out.append(f"            // 0x{o:X},")
        return "\n".join(out)

    cs = f"""using WowPacketParser.Misc;

namespace WowPacketParser.Enums.Version.{out_ver}
{{
    public static class {cls_name}
    {{
        public static BiDictionary<Opcode, int> Opcodes(Direction direction)
        {{
            switch (direction)
            {{
                case Direction.ClientToServer:
                    return ClientOpcodes;
                case Direction.ServerToClient:
                    return ServerOpcodes;
                default:
                    return MiscOpcodes;
            }}
        }}

        private static readonly BiDictionary<Opcode, int> ClientOpcodes = new()
        {{
{block(cdict, un_c)}
        }};

        private static readonly BiDictionary<Opcode, int> ServerOpcodes = new()
        {{
{block(sdict, un_s)}
        }};

        private static readonly BiDictionary<Opcode, int> MiscOpcodes = new();
    }}
}}
"""
    path = os.path.join(outdir, 'Opcodes.cs')
    open(path, 'w', encoding='utf8').write(cs)
    print(f"[+] wrote {path}")
    print(f"    named C={len(cdict)} S={len(sdict)} | unnamed C={len(un_c)} S={len(un_s)}")

if __name__ == '__main__':
    main()
