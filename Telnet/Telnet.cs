using System;
using System.Net.Sockets;
using System.Text;
using ServidorTelnet.Extensions;

namespace ServidorTelnet.Telnet
{
    public abstract class Telnet
    {
        private readonly Encoding _codificacao = Encoding.ASCII;

        private const byte CR = 13;
        private const byte LF = 10;
        private const byte NUL = 0;

        public enum EOLType
        {
            CRLF = 0,
            CRNUL = 1,
            LF = 2
        }

        public EOLType EOL = EOLType.CRLF;

        // -----------------------------------------------------------------------------------------------------------------

        protected bool Escrever(NetworkStream stream, byte[] cmd)
        {
            if (stream == null) return false;
            if (!stream.CanWrite) return false;
            try
            {
                stream.Write(cmd, 0, cmd.Length);
                stream.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected bool Escrever(NetworkStream networkStream, string mensagem, bool quebraLinha = true)
        {
            if (quebraLinha)
                if (!EscreverEol(networkStream)) return false;
            return Escrever(networkStream, _codificacao.GetBytes(mensagem.Replace("\0xFF", "\0xFF\0xFF")));
        }
        private bool EscreverEol(NetworkStream stream)
        {
            switch (EOL)
            {
                case EOLType.CRLF:
                    return Escrever(stream, new byte[] { CR, LF });
                case EOLType.CRNUL:
                    return Escrever(stream, new byte[] { CR, NUL });
                case EOLType.LF:
                    return Escrever(stream, new byte[] { LF });
                default:
                    return false;
            }
        }


        // -----------------------------------------------------------------------------------------------------------------

        protected string[] Ler(NetworkStream stream, Cliente cliente)
        {
            var stringBuilder = new StringBuilder();
            while (stream != null && (stream.CanRead))
            {
                HandShaking(stream, stringBuilder, cliente);

                var comandos = stringBuilder.ToString();
                if (comandos.Contains((char)CR, (char)LF, '\r', '\n', '>'))
                {
                    var linhas = comandos.Split(new char[] { (char)CR, (char)LF, (char)NUL, '\r', '\n', '>' }, StringSplitOptions.RemoveEmptyEntries);
                    return (linhas.Length > 0) ? linhas : new[] { "" };
                }
            }
            return null;
        }

        void HandShaking(NetworkStream stream, StringBuilder stringBuilder, Cliente cliente)
        {
            do
            {
                while (stream.DataAvailable)
                {
                    var entrada = (Verbs)stream.ReadByte();
                    switch (entrada)
                    {
                        case Verbs.UNKNOWN:
                            break;
                        case Verbs.IAC:
                            cliente.PuTTy = true;
                            var verbo = (Verbs)stream.ReadByte();
                            switch (verbo)
                            {
                                case Verbs.UNKNOWN:
                                    break;
                                case Verbs.IAC:
                                    stringBuilder.Append(verbo);
                                    break;
                                case Verbs.DO:
                                case Verbs.DONT:
                                case Verbs.WILL:
                                case Verbs.WONT:
                                    var opcao = (Verbs)stream.ReadByte();

                                    if (opcao < 0)
                                        break;

                                    stream.WriteByte((byte)Verbs.IAC);
                                    switch (opcao)
                                    {
                                        case Verbs.SGA:
                                            stream.WriteByte(verbo == Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                            break;
                                        default:
                                            stream.WriteByte(verbo == Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                            break;
                                    }
                                    stream.WriteByte((byte)opcao);
                                    break;
                            }
                            break;
                        default:
                            stringBuilder.Append((char)entrada);
                            break;
                    }
                }
            }
            while (stream.DataAvailable);
        }


        // -------------------------------------------------------------------------------------------------------------------
        public string ObterCifrao() => Colorido(Cores.Verde, "$: ", true);
        public string ObterTitulo(string titulo) => $"{ControlCharacters.OSC}{OperatingSystemControls.ChangeWindowTitletoPt};{titulo}{ASCII.BELL}";

        //   ASCII
        // ESC    1B
        // BELL   07

        struct ASCII
        {
            public const string ESC = "\x1b";
            public const string BELL = "\x07";
        }

        struct ControlCharacters
        {
            public const string OSC = ASCII.ESC + "]";
            public const string CSI = ASCII.ESC + "[";
        }

        struct OperatingSystemControls
        {
            public const string ChangeWindowTitletoPt = "2";
        }


        public enum Cores
        {
            Preto = 30,
            Vermelho = 31,
            Verde = 32,
            Amarelo = 33,
            Azul = 34,
            Magenta = 35,
            Ciano = 36,
            Branco = 37
        }

        public static string Colorido(Cores cor, string mensagem, bool brilho = false) => ControlCharacters.CSI + ((int)cor) + (brilho ? ";1m" : "m") + mensagem + ControlCharacters.CSI + "0m";
    }
}
