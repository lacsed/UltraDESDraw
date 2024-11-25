namespace UltraDESDraw.Services 
{
    public class FlowChartData
    {
        public List<Automaton> Automata {get; set; } = new List<Automaton>();
        public List<int> Operations {get; set; } = new List<int>();
        public List<int> Connections {get; set; } = new List<int>();
    }
}