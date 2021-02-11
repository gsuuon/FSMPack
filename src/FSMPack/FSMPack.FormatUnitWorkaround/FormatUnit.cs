using System;
using Microsoft.FSharp.Core;

using FSMPack.Spec;
using static FSMPack.Format;
using static FSMPack.Read;
using static FSMPack.Write;

namespace FSMPack.FormatUnitWorkaround
{
    public class FormatUnit : Format<Unit>
    {
        public void Write(Write.BufWriter bw, Unit unit) {
            writeValue(bw, Value.Nil);
        }
        public Unit Read(Read.BufReader br, ReadOnlySpan<byte> bytes) {
            var nil = readValue(br, ref bytes);
            if (!Value.Nil.Equals(nil)) {
                throw new FormatException("Expected Nil");
            }
            return null;
        }

        public static void StoreFormat() {
            Cache<Unit>.Store(new FormatUnit());
        }
    }
}
