using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace FalconBMS.Launcher.Input
{
    public sealed class KeyFile : ICloneable
    {
        public KeyAssgn[] keyAssign;

        public string[] categoryHeaderLabels;

        public KeyFile(string filename)
        {
            // Verify file exists.
            if (File.Exists(filename) == false)
            {
                MessageBoxResult result = MessageBox.Show
                    ("App could not find " + filename, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Build table of { callbackName, keyBinding, descriptionString }.  Also build up list of category-header labels.
            List<KeyAssgn> records = new List<KeyAssgn>(2000);

            List<string> cats = new List<string>(12);

            using (StreamReader reader = File.OpenText(filename))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (RegexFactory.LineComment.IsMatch(line))
                        continue;

                    // Ignore any DX button/hat bindings -- we're only interested in keyboard bindings, here.
                    if (RegexFactory.ButtonOrHatBindingLine.IsMatch(line))
                        continue;

                    if (RegexFactory.CategoryHeaderLine.IsMatch(line))
                        cats.Add(ParseCategoryHeaderLabel(line)); //nb: also fall-through to add it to KeyAssgn

                    // Parse the key-binding line.
                    KeyAssgn keyAssgn = ParseKeyfileLine(line);
                    records.Add(keyAssgn);
                }
            }

            keyAssign = records.ToArray();
            categoryHeaderLabels = cats.ToArray();
            return;
        }

        private static string ParseCategoryHeaderLabel(string line)
        {
            Match m = RegexFactory.CategoryHeaderLine.Match(line);
            Debug.Assert(m.Success);

            string cat = m.Groups["categoryHeaderDQ"].Value;
            return cat;
        }

        internal static KeyAssgn ParseKeyfileLine(string line)
        {
            // Ignore any DX button/hat bindings -- we're only interested in keyboard bindings, here.
            if (RegexFactory.ButtonOrHatBindingLine.IsMatch(line))
                return null;

            // Parse the key-binding line.
            Regex keyBindingLineRx = RegexFactory.KeyBindingLine;
            Match m = keyBindingLineRx.Match(line);
            if (!m.Success)
                throw new InvalidDataException("Unexpected line in keyfile: " + line);

            string callbackName = m.Groups["callbackName"].Value;
            string soundId = m.Groups["soundId"].Value;
            string keyScancodeHex = m.Groups["keyScancodeHex"].Value;
            string keyModifierFlags = m.Groups["keyModifierFlags"].Value;
            string chordScancodeHex = m.Groups["chordScancodeHex"].Value;
            string chordModifierFlags = m.Groups["chordModifierFlags"].Value;
            string displayFlags = m.Groups["displayFlags"].Value;
            string descriptionStringDQ = m.Groups["descriptionStringDQ"].Value;

            return new KeyAssgn(
                callbackName, soundId, "0", keyScancodeHex, keyModifierFlags, chordScancodeHex, chordModifierFlags, displayFlags, descriptionStringDQ
            );
        }

        public KeyFile(IReadOnlyList<KeyAssgn> keyAssign)
        {
            this.keyAssign = new KeyAssgn[keyAssign.Count];
            for (int i = 0; i < keyAssign.Count; i++)
                this.keyAssign[i] = keyAssign[i].Clone();
        }

        object ICloneable.Clone() => Clone();

        public KeyFile Clone()
        {
            return new KeyFile(keyAssign);
        }

        internal static class RegexFactory
        {
            static RegexFactory()
            {
#if DEBUG
                SelfTest();
#endif
            }

            public static Regex LineComment = Create(@"(?nsx) #ExplicitCapture, Singleline, IgnorePatternWhitespace
                ^
                    \s* \x23 .* # \x23: number symbol
                $"
            );

            public static Regex CategoryHeaderLine = Create(@"(?nsx) #ExplicitCapture, Singleline, IgnorePatternWhitespace
                ^\s*
                    SimDoNothing \s+ -1 \s+ 0 \s+ 0[xX]FFFFFFFF \s+ 0 \s+ 0 \s+ 0 \s+ -1 \s+ (?<categoryHeaderDQ>\x22 \d+\.\s [^\x22]+ \x22)
                \s*$"
            );

            public static Regex KeyBindingLine = Create(@"(?nsx) #ExplicitCapture, Singleline, IgnorePatternWhitespace
                ^\s*
                    (?<callbackName> [A-Za-z0-9_]+ )
                \s+
                    (?<soundId> \x2D?\d+ ) # \x2D: minus symbol
                \s+
                    (?<unused0> 0 )
                \s+
                    (?<keyScancodeHex> 0([xX][0-9A-Fa-f]{1,8})? )
                \s+
                    (?<keyModifierFlags> [0-7] )
                \s+
                    (?<chordScancodeHex> 0([xX][0-9A-Fa-f]{1,8})? )
                \s+
                    (?<chordModifierFlags> [0-7] )
                \s+
                    (?<displayFlags> \x2D?\d ) # \x2D: minus symbol
                \s+
                    (?<descriptionStringDQ> \x22.*\x22 ) # \x22: doublequote char
                    (
                        \s*
                        (?<trailingComment> \x23 .* ) # optional trailing comment
                    )?
                \s*$"
            );

            public static Regex ButtonOrHatBindingLine = Create(@"(?nsx) #ExplicitCapture, Singleline, IgnorePatternWhitespace
                ^\s*
                    (?<callbackName> [A-Za-z0-9_]+ )
                \s+
                    (?<bmsButtonId> \d+ )
                \s+
                    (?<invocatonBehavior> (\x2D[124])|8 ) # -1, -2, -4, or 8
                \s+
                    (?<buttonOrHat> \x2D[23] ) # -2: button, -3: pov-hat
                \s+
                    (?<pressOrRelease> (0|0x42|[0-7]) ) # 0: press, 0x42: release (or [0-7] for 8-way hat direction)
                \s+
                    (?<unused> 0x\d ) # usually 0x0 but sometimes 0x[1-8] in older stock keyfiles
                    (
                        \s+
                        (?<soundId> (-1|\d+) ) # optional
                    )?
                    (
                        \s+
                        (?<descriptionStringDQ> \x22.*\x22 ) # optional, \x22: doublequote char
                    )?
                    (
                        \s*
                        (?<trailingComment> \x23 .* ) # optional trailing comment
                    )?
                \s*$"
            );

            private static Regex Create(string pattern)
            {
                RegexOptions defaultOpts = RegexOptions.CultureInvariant | RegexOptions.Compiled;
                return new Regex(pattern, defaultOpts);
            }

            private static void SelfTest()
            {
#if DEBUG
                Debug.Assert(LineComment.IsMatch(@"# foo"));
                Debug.Assert(LineComment.IsMatch(@"  ## foo  "));

                Debug.Assert(CategoryHeaderLine.IsMatch(@"SimDoNothing -1 0 0XFFFFFFFF 0 0 0 -1 ""1. UI & 3RD PARTY SOFTWARE"""));
                Debug.Assert(false == CategoryHeaderLine.IsMatch(@"SimDoNothing -1 0 0XFFFFFFFF 0 0 0 -1 ""======== 1.02     3RD PARTY SOFTWARE ========"""));

                Debug.Assert(KeyBindingLine.IsMatch(@"SimDoNothing -1 0 0xFFFFFFFF 0 0 0 -0 ""REM: party keys. Avoid them in your key file"""));
                Debug.Assert(KeyBindingLine.IsMatch(@"SimAltFlaps 311 0 0x3C 6 0 0 1 ""FLT: ALT FLAPS Switch - Toggle"""));
                Debug.Assert(KeyBindingLine.IsMatch(@"SimF15NoseGearSteering -1 0 0XFFFFFFFF 0 0 0 1 ""F-15: Nose Gear Steering"""));
                Debug.Assert(KeyBindingLine.IsMatch(@"OTWToggleFrameRate -1 0 0x21 0 0x2E 4 1 ""SIM: Display Frame Rate - Toggle"""));
                Debug.Assert(KeyBindingLine.IsMatch(@"SimIFFBackupM1Digit1_0 312 0 0XFFFFFFFF 0 0 0 1 ""AUX: IFF MODE I - 0* **"""));
                Debug.Assert(KeyBindingLine.IsMatch(@"SimDoNothing -1 0 0xFFFFFFFF 0 0 0 -0 ""Test: optional trailing comment""#foo"));
                Debug.Assert(KeyBindingLine.IsMatch(@"SimDoNothing -1 0 0xFFFFFFFF 0 0 0 -0 ""Test: optional trailing comment"" # foo "));

                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"SimHookToggle 123 -1 -2 0 0x0 0"));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"SimTrimAPDisc 42 -2 -2 0 0x0 0"));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"SimTrimAPDisc 42 -2 -2 0x42 0x0 0"));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"AFElevatorTrimUp 2 -1 -3 0 0x0"));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"AFElevatorTrimUp 2 -1 -3 0 0x0 0"));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"AFElevatorTrimUp 2 -1 -3 0 0x0 -1"));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"AFElevatorTrimUp 2 -1 -3 0 0x0 -1 ""optional description"""));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"SimDoNothing 1 -1 -2 0 0x0 0#foo"));
                Debug.Assert(ButtonOrHatBindingLine.IsMatch(@"SimDoNothing 1 -1 -2 0 0x0 0 # foo "));
#endif
            }
        }

    }
}
