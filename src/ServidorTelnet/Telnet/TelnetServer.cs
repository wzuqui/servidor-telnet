using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServidorTelnet.Telnet
{
    public class TelnetServer : Telnet
    {
        private TcpListener _tcpListener;
        private Thread _thread;
        private readonly ConcurrentDictionary<Guid, Cliente> _clientes;

        public TelnetServer()
        {
            _clientes = new ConcurrentDictionary<Guid, Cliente>();
        }

        public void Iniciar(IPAddress ipAddress, int port = 23)
        {
            _tcpListener = new TcpListener(ipAddress, port);
            _tcpListener.Start();

            _thread = new Thread(Processar) {IsBackground = true};
            _thread.Start();
        }

        private void Processar()
        {
            while (_tcpListener != null)
            {
                TcpClient tcpClient;

                try
                {
                    tcpClient = _tcpListener.AcceptTcpClient();
                }
                catch
                {
                    tcpClient = null;
                }
                if (tcpClient != null && tcpClient.Connected)
                {
                    _clientes.AddOrUpdate(Guid.NewGuid(), guid =>
                    {
                        var thread = new Thread(ClienteProcesso)
                        {
                            IsBackground = true
                        };

                        var cliente = new Cliente(guid, tcpClient, thread);
                        thread.Start(cliente);
                        return cliente;
                    }, (guid, cliente) => cliente);
                }
            }
        }

        private void ClienteProcesso(object obj)
        {
            var cliente = (Cliente)obj;

            cliente.BemVindo();

            while (cliente.Conectado)
            {
                foreach (var comando in cliente.LerComando())
                    if (!string.IsNullOrWhiteSpace(comando))
                        TratarComando(cliente, comando);
                    else
                        cliente.Escrever(string.Empty, false);
            }
        }

        private void TratarComando(TelnetClient cliente, string comando)
        {
            cliente.Escrever($"Comando digitado: {comando}");

            if (comando.Equals("sair"))
                cliente.Desconectar();
        }

        public void Parar()
        {
            if (_tcpListener != null)
                try { _tcpListener.Stop(); }
                catch
                {
                    // ignored
                }

            if (_thread != null)
                try { _thread.Abort(); }
                catch
                {
                    // ignored
                }

            DesconectarTodos();
        }

        private void DesconectarTodos()
        {
            foreach (var cliente in _clientes)
                Desconectar(cliente.Key);
        }

        private void Desconectar(Guid clienteGuid)
        {
            if (_clientes.TryRemove(clienteGuid, out var cliente))
                cliente.Desconectar();
        }

        public Task<TcpClient> AcceptAsync()
        {
            return _tcpListener.AcceptTcpClientAsync();
        }
    }
}
