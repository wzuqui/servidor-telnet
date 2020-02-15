using System;
using System.Net.Sockets;
using System.Threading;

namespace ServidorTelnet.Telnet
{
    public class Cliente : Telnet
    {
        public Guid guid;
        public bool PuTTy;

        private TcpClient _tpcClient;
        private Thread _thread;

        public Cliente(Guid guid, TcpClient tpcClient, Thread thread)
        {
            this.guid = guid;
            this._tpcClient = tpcClient;
            this._thread = thread;
        }

        internal bool Connected => _tpcClient?.Connected == true;

        internal void Desconectar()
        {
            try { _thread.Abort(); } catch { }
            try { _tpcClient.Close(); } catch { }
        }

        internal void BemVindo()
        {
            Escrever(_tpcClient.GetStream(), @"
  ____                                 _               _         
 | __ )    ___   _ __ ___     __   __ (_)  _ __     __| |   ___  
 |  _ \   / _ \ | '_ ` _ \    \ \ / / | | | '_ \   / _` |  / _ \ 
 | |_) | |  __/ | | | | | |    \ V /  | | | | | | | (_| | | (_) |
 |____/   \___| |_| |_| |_|     \_/   |_| |_| |_|  \__,_|  \___/ 
                                                                 ");
            Escrever(_tpcClient.GetStream(), $"Cliente id {guid}");
        }
        internal bool MudarTitulo(string titulo) => PuTTy && Escrever(_tpcClient.GetStream(), ObterTitulo(titulo), false);
        internal bool Escrever(string mensagem, bool quebraLinha = true) => Escrever(_tpcClient.GetStream(), mensagem, quebraLinha);

        internal string[] LerComando()
        {
            Escrever(ObterCifrao());
            MudarTitulo($"Cliente id {guid} - Ultimo comando {DateTime.Now.ToString()}");

            return Ler(_tpcClient.GetStream(), this);
        }
        internal string[] Ler() => Ler(_tpcClient.GetStream(), this);

    }
}