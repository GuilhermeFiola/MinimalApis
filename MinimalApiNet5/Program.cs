using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var webHost = new WebHostBuilder()
    .UseKestrel()
    .ConfigureServices(services => {
        services.AddRouting();
        services.AddSingleton<ClienteRepository>();
    })
    .Configure(app => {

        app.UseRouting();

        // .NET Core 3
        var repo = app.ApplicationServices.GetService<ClienteRepository>();
        
        app.UseEndpoints(endpoints => {
            
            endpoints.MapGet("/clientes", context => {
                var clientes = repo.GetAll();
                return context.Response.WriteAsJsonAsync(clientes);
            });

            endpoints.MapGet("/clientes/{id}", context => {
                context.Request.RouteValues.TryGetValue("id", out var id);
                var idGuid = Guid.Parse(id.ToString());
                var cliente = repo.GetById(idGuid);

                if (cliente is null) {
                    context.Response.StatusCode = 404;
                    return context.Response.CompleteAsync();
                }

                return context.Response.WriteAsJsonAsync(cliente);
            });

            endpoints.MapPost("/clientes", context => {
                var bodyStr = "";
                var req = context.Request;

                using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = Task.Run(async () => await reader.ReadToEndAsync()).Result;
                }
                
                var cliente = JsonSerializer.Deserialize<Cliente>(bodyStr);
                repo.Create(cliente);

                context.Response.StatusCode = 201;
                return context.Response.WriteAsJsonAsync(cliente);
            });

            endpoints.MapPut("/clientes/{id}", context => {
                context.Request.RouteValues.TryGetValue("id", out var id);
                var idGuid = Guid.Parse(id.ToString());
                
                var cliente = repo.GetById(idGuid);
                
                if (cliente is null) {
                    context.Response.StatusCode = 404;
                    return context.Response.CompleteAsync();
                }

                var bodyStr = "";
                var req = context.Request;

                using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = Task.Run(async () => await reader.ReadToEndAsync()).Result;
                }
                
                var clienteUpdate = JsonSerializer.Deserialize<Cliente>(bodyStr);

                repo.Update(clienteUpdate);

                return context.Response.WriteAsJsonAsync(clienteUpdate);
            });

            endpoints.MapDelete("/clientes/{id}", context => {
                context.Request.RouteValues.TryGetValue("id", out var id);
                var idGuid = Guid.Parse(id.ToString());

                repo.Delete(idGuid);

                return context.Response.CompleteAsync();
            });
        });
    })
    .Build();

webHost.Run();

record Cliente(Guid Id, string NomeCompleto);

class ClienteRepository
{
    private readonly Dictionary<Guid, Cliente> _clientes = new();

    public void Create(Cliente cliente) {
        if (cliente is null) return;
        _clientes[cliente.Id] = cliente;
    }

    public Cliente GetById(Guid id) {
        var retorno = _clientes.TryGetValue(id, out Cliente cliente);
        return cliente;
    }

    public List<Cliente> GetAll() {
        return _clientes.Values.ToList();
    }

    public void Update(Cliente cliente) {
        var clienteExistente = GetById(cliente.Id);
        if (clienteExistente is null) return;
        _clientes[cliente.Id] = cliente;
    }

    public void Delete(Guid id) {
        _clientes.Remove(id);
    }
}