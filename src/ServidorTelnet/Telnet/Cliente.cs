using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;

namespace ServidorTelnet.Telnet
{
    public class Cliente : TelnetClient
    {
        private readonly Guid _guid;

        public Cliente(Guid guid, TcpClient tpcClient, Thread thread) : base(tpcClient, thread)
        {
            _guid = guid;
        }

        internal void BemVindo()
        {
            Escrever(@"
  ____                                 _               _         
 | __ )    ___   _ __ ___     __   __ (_)  _ __     __| |   ___  
 |  _ \   / _ \ | '_ ` _ \    \ \ / / | | | '_ \   / _` |  / _ \ 
 | |_) | |  __/ | | | | | |    \ V /  | | | | | | | (_| | | (_) |
 |____/   \___| |_| |_| |_|     \_/   |_| |_| |_|  \__,_|  \___/ 
                                                                 ");
            Escrever($"Cliente id {_guid}");
        }


        public IEnumerable<string> LerComando()
        {
            Escrever(TerminalComando.Cifrao);

            if (Negociado)
                MudarTitulo($"Cliente id {_guid} - Ultimo comando {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");

            return Ler();
        }
    }
}