using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ServidorTelnet.Telnet
{
    public abstract class TelnetClient : Telnet
    {
        private readonly TcpClient _tpcClient;
        private readonly Thread _thread;

        protected TelnetClient(TcpClient tpcClient, Thread thread)
        {
            _tpcClient = tpcClient;
            _thread = thread;
        }

        public bool Conectado => _tpcClient?.Connected == true;
        public void Desconectar()
        {
            try
            {
                _thread.Abort();
            }
            catch
            {
                // ignored
            }

            try
            {
                _tpcClient.Close();
            }
            catch
            {
                // ignored
            }
        }
        public void Escrever(string mensagem, bool quebraLinha = true) => Escrever(_tpcClient.GetStream(), mensagem, quebraLinha);

        protected IEnumerable<string> Ler() => Ler(_tpcClient.GetStream());
        protected void MudarTitulo(string titulo) => Escrever(TerminalComando.TituloDaJanela(titulo), false);
    }
}