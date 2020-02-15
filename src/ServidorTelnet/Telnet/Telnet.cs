using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ServidorTelnet.Extensions;

#pragma warning disable 162

namespace ServidorTelnet.Telnet
{
    public abstract class Telnet
    {
        private readonly Encoding _codificacao = Encoding.ASCII;
        private const EolTipos EOL = EolTipos.CRLF;
        protected bool Negociado { get; private set; }

        private enum EolTipos : byte
        {
            NUL = 0,
            CR = 13,
            LF = 10,
            CRNUL = CR,
            CRLF = CR & LF
        }
        private enum Verbos
        {
            DESCONHECIDO = -1,
            SGA = 3,
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255
        }

        protected void Escrever(NetworkStream networkStream, string mensagem, bool quebraLinha = true)
        {
            if (quebraLinha)
                _escreverQuebraLinha(networkStream);
            _escreverBytes(networkStream, _codificacao.GetBytes(mensagem.Replace("\0xFF", "\0xFF\0xFF")));
        }

        protected IEnumerable<string> Ler(NetworkStream stream)
        {
            var stringBuilder = new StringBuilder();
            while (stream != null && (stream.CanRead))
            {
                HandShaking(stream, stringBuilder);

                var comandos = stringBuilder.ToString();
                if (!comandos.Contains((char) EolTipos.CR, (char) EolTipos.LF, '\r', '\n', '>')) continue;

                var linhas =
                    comandos.Split(new[] {(char) EolTipos.CR, (char) EolTipos.LF, (char) EolTipos.NUL, '\r', '\n', '>'},
                        StringSplitOptions.RemoveEmptyEntries);
                return linhas.Length > 0 ? linhas : new[] {string.Empty};
            }

            return null;
        }

        private void _escreverBytes(Stream stream, byte[] cmd)
        {
            if (stream == null) return;
            if (!stream.CanWrite) return;
            try
            {
                stream.Write(cmd, 0, cmd.Length);
                stream.Flush();
            }
            catch
            {
                // ignored
            }
        }

        private void _escreverQuebraLinha(Stream stream)
        {
            switch (EOL)
            {
                case EolTipos.CRLF:
                    _escreverBytes(stream, new[] {(byte) EolTipos.CR, (byte) EolTipos.LF});
                    break;
                case EolTipos.CRNUL:
                    _escreverBytes(stream, new[] {(byte) EolTipos.CR, (byte) EolTipos.NUL});
                    break;
                case EolTipos.LF:
                    _escreverBytes(stream, new[] {(byte) EolTipos.LF});
                    break;
            }
        }

        private void HandShaking(NetworkStream stream, StringBuilder stringBuilder)
        {
            do
            {
                while (stream.DataAvailable)
                {
                    var token = (Verbos) stream.ReadByte();
                    switch (token)
                    {
                        case Verbos.DESCONHECIDO:
                            break;
                        case Verbos.IAC:
                            Negociado = true;
                            var verbo = (Verbos) stream.ReadByte();
                            switch (verbo)
                            {
                                case Verbos.DESCONHECIDO:
                                    break;
                                case Verbos.IAC:
                                    stringBuilder.Append(verbo);
                                    break;
                                case Verbos.DO:
                                case Verbos.DONT:
                                case Verbos.WILL:
                                case Verbos.WONT:
                                    var opcao = (Verbos) stream.ReadByte();

                                    if (opcao < 0)
                                        break;

                                    stream.WriteByte((byte) Verbos.IAC);
                                    switch (opcao)
                                    {
                                        case Verbos.SGA:
                                            stream.WriteByte(verbo == Verbos.DO
                                                ? (byte) Verbos.WILL
                                                : (byte) Verbos.DO);
                                            break;
                                        default:
                                            stream.WriteByte(verbo == Verbos.DO
                                                ? (byte) Verbos.WONT
                                                : (byte) Verbos.DONT);
                                            break;
                                    }

                                    stream.WriteByte((byte) opcao);
                                    break;
                            }

                            break;
                        default:
                            stringBuilder.Append((char) token);
                            break;
                    }
                }
            } while (stream.DataAvailable);
        }
    }
}