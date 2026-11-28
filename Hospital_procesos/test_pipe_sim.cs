using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Iniciando prueba de Named Pipe...");
        string pipeName = "TestHospitalPipe_" + Guid.NewGuid().ToString("N");

        var serverTask = Task.Run(async () =>
        {
            using var server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            Console.WriteLine("[Server] Esperando conexion...");
            await server.WaitForConnectionAsync();
            Console.WriteLine("[Server] Cliente conectado!");

            var reader = new StreamReader(server);
            var writer = new StreamWriter(server) { AutoFlush = true };

            string? line = await reader.ReadLineAsync();
            Console.WriteLine($"[Server] Recibido: {line}");

            string response = "{\"idSolicitud\":\"SOL-1\",\"estado\":\"Procesada correctamente\",\"resultados\":[]}";
            Console.WriteLine("[Server] Enviando respuesta...");
            writer.WriteLine(response);
            writer.Flush();

            try
            {
                Console.WriteLine("[Server] WaitForPipeDrain...");
                server.WaitForPipeDrain();
                Console.WriteLine("[Server] Pipe drenado!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error drain: {ex.Message}");
            }

            Console.WriteLine("[Server] Desconectando...");
            server.Disconnect();
            Console.WriteLine("[Server] Fin.");
        });

        await Task.Delay(200);

        Console.WriteLine("[Client] Conectando...");
        using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(3000);
        Console.WriteLine("[Client] Conectado!");

        using var clientWriter = new StreamWriter(client) { AutoFlush = true };
        using var clientReader = new StreamReader(client);

        Console.WriteLine("[Client] Enviando mensaje...");
        await clientWriter.WriteLineAsync("{\"solicitud\":\"test\"}");

        Console.WriteLine("[Client] Esperando respuesta...");
        using var cts = new CancellationTokenSource(5000);
        string? resp = await clientReader.ReadLineAsync(cts.Token);
        Console.WriteLine($"[Client] Respuesta recibida: {resp}");

        await serverTask;
        Console.WriteLine("TEST FINALIZADO CON EXITO!");
    }
}
