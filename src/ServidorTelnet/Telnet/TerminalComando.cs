// ReSharper disable UnusedMember.Local
namespace ServidorTelnet.Telnet
{
    public struct TerminalComando
    {
        private const string ESC = "\x1b";
        private const string BELL = "\x07";

        private const string IND = ESC + "D";
        private const string NEL = ESC + "E";
        private const string HTS = ESC + "H";
        private const string RI = ESC + "M";
        private const string SS2 = ESC + "N";
        private const string SS3 = ESC + "O";
        private const string DCS = ESC + "P";
        private const string SPA = ESC + "V";
        private const string EPA = ESC + "W";
        private const string SOS = ESC + "X";
        private const string DECID = ESC + "Z";
        private const string CSI = ESC + "[";
        private const string ST = ESC + "\\";
        private const string OSC = ESC + "]";
        private const string PM = ESC + "^";
        private const string APC = ESC + "_";

        public static string Cifrao => 
            Cor(Cores.VERDE, "$: ", true);

        public static string TituloDaJanela(string titulo) =>
            $"{OSC}2;{titulo}{BELL}";

        private static string Cor(Cores cor, string mensagem, bool brilho = false) =>
            $"{CSI}{(int) cor}{(brilho ? ";1m" : "m")}{mensagem}{CSI}{(int) Cores.NORMAL}m";
        
        private enum Cores
        {
            NORMAL = 0,
            PRETO = 30,
            VERMELHO = 31,
            VERDE = 32,
            AMARELO = 33,
            AZUL = 34,
            MAGENTA = 35,
            CIANO = 36,
            BRANCO = 37
        }
    }
}