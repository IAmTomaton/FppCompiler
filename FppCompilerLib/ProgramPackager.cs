using FppCompilerLib.CodeGeneration;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace FppCompilerLib
{
    internal class ProgramPackager
    {
        private static readonly string[] signalNames = Enumerable.Range(0, 10)
            .Select(d => d.ToString())
            .Concat("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(ch => ch.ToString()))
            .Select(s => $"signal-{s}")
            .ToArray();
        private static readonly string addressSignal = "signal-blue";
        private static readonly int signalsPerCommand = 4;
        private static readonly int signalsInCombinator = 240;
        private static readonly int commandsInCell = signalNames.Length / signalsPerCommand;
        private static readonly int commandsInCombinator = signalsInCombinator / signalsPerCommand;
        private static readonly int combinatorsInCell = commandsInCell / commandsInCombinator + 1;

        private static readonly int columnShift = 7;
        private static readonly int columnLength = 64;

        public ProgramPackager() { }

        public string PackProgram(MachineCommand[] program, bool add_address=false)
        {
            var combinators = CommandsToCombinators(program, add_address);
            var blueprint = new Blueprint(combinators);
            var blueprintString = BlueprintToString(blueprint);
            return blueprintString;
        }

        private Combinator[] CommandsToCombinators(MachineCommand[] commands, bool add_address)
        {
            return commands.Select((cmd, index) => new
            {
                cmd,
                cellIndex = index / commandsInCell,
                combinatorIndex = index % commandsInCell / commandsInCombinator,
                commandIndex = index % commandsInCell % commandsInCombinator
            })
            .GroupBy(item => item.cellIndex * combinatorsInCell + item.combinatorIndex)
            .Select(comb => comb.SelectMany(item => CommandToSignals(item.cmd, item.combinatorIndex, item.commandIndex)))
            .Select((signals, index) => add_address ? signals.Append(new Signal(new SignalInfo { type = "virtual", name = addressSignal }, signalNames.Length, -index / combinatorsInCell - 1)) : signals)
            .Select((signals, index) => new Combinator(signals.ToArray(), index / combinatorsInCell, index % combinatorsInCell))
            .ToArray();
        }

        private Signal[] CommandToSignals(MachineCommand command, int combinatorIndex, int commandIndex)
        {
            var numbers = CommandToNumbers(command);
            var signals = NumbersToSignals(numbers, combinatorIndex, commandIndex);
            return signals;
        }

        private int[] CommandToNumbers(MachineCommand command)
        {
            (var unitId, var cmdId) = command.id;
            var instructionId = (unitId << 16) + cmdId;
            return new int[] { instructionId }.Concat(command.args).ToArray();
        }

        private Signal[] NumbersToSignals(int[] numbers, int combinatorIndex, int commandIndex)
        {
            var placeOffset = commandIndex * signalsPerCommand;
            var signalsOffset = combinatorIndex * signalsInCombinator + placeOffset;
            return numbers.Select((s, i) => new { num = s, index = i })
                .Select(item => new Signal(signalsOffset + item.index, placeOffset + item.index, item.num))
                .ToArray();
        }

        private string BlueprintToString(Blueprint blueprint)
        {
            var bluepringtJson = JsonSerializer.Serialize(blueprint);
            var bluepringtEncoded = Encoding.UTF8.GetBytes(bluepringtJson);
            var compressed = CompressZLib(bluepringtEncoded);
            var base64String = Convert.ToBase64String(compressed);
            return "0" + base64String;
        }

        private static byte[] CompressZLib(byte[] bytes)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new ZLibStream(memoryStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }
            return memoryStream.ToArray();
        }

        private struct Signal
        {
            public SignalInfo signal { get; set; }
            public int count { get; set; }
            public int index { get; set; }

            public Signal(int nameIndex, int signalIndex, int count)
            {
                this.count = count;
                index = signalIndex + 1;
                signal = new SignalInfo
                {
                    type = "virtual",
                    name = signalNames[nameIndex],
                };
            }

            public Signal(SignalInfo info, int signalIndex, int count)
            {
                this.count = count;
                index = signalIndex + 1;
                signal = info;
            }
        }

        private struct SignalInfo
        {
            public string type { get; set; }
            public string name { get; set; }
        }

        private struct Combinator
        {
            public int entity_number { get; set; }
            public string name { get; set; }
            public Position position { get; set; }
            public int direction { get; set; }
            public ControlBehavior control_behavior { get; set; }

            public Combinator(Signal[] signals, int cellIndex, int combinatorIndex)
            {
                entity_number = cellIndex * combinatorsInCell + combinatorIndex;
                name = "constant-combinator";
                position = new Position
                {
                    x = cellIndex / columnLength * columnShift + combinatorIndex,
                    y = -cellIndex
                };
                direction = 2;
                control_behavior = new ControlBehavior
                {
                    filters = signals
                };
            }
        }

        private struct Position
        {
            public int x { get; set; }
            public int y { get; set; }
        }

        private struct ControlBehavior
        {
            public Signal[] filters { get; set; }
        }

        private struct Blueprint
        {
            public BlueprintInfo blueprint { get; set; }

            public Blueprint(Combinator[] combinators)
            {
                blueprint = new BlueprintInfo
                {
                    icons = new Signal[] {
                        new Signal
                        {
                            count = 0,
                            index = 1,
                            signal = new SignalInfo
                            {
                                type = "item",
                                name = "constant-combinator",
                            }
                        }
                    },
                    entities = combinators,
                    item = "blueprint",
                    version = 281479275544576
                };
            }
        }

        private struct BlueprintInfo
        {
            public Signal[] icons { get; set; }
            public Combinator[] entities { get; set; }
            public string item { get; set; }
            public long version { get; set; }
        }
    }
}
