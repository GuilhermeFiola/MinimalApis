using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ClienteRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();

app.MapGet("/clientes", ([FromServices] ClienteRepository repo) => 
{
    return repo.GetAll();
});

app.MapGet("/clientes/{id}", ([FromServices] ClienteRepository repo, Guid id) => 
{
    var cliente = repo.GetById(id);
    return cliente is not null ? Results.Ok(cliente) : Results.NotFound();
});

app.MapPost("/clientes", ([FromServices] ClienteRepository repo, Cliente cliente) => 
{
    repo.Create(cliente);
    return Results.Created($"/clientes/{cliente.Id}", cliente);
});

app.MapPut("/clientes/{id}", ([FromServices] ClienteRepository repo, Guid id, Cliente clienteUpdate) => 
{
    var cliente = repo.GetById(id);
    if (cliente is null) return Results.NotFound();
    repo.Update(clienteUpdate);
    return Results.Ok(clienteUpdate);
});

app.MapDelete("/clientes/{id}", ([FromServices] ClienteRepository repo, Guid id) => 
{
    repo.Delete(id);
    return Results.Ok();
});

app.UseSwaggerUI();

app.Run();

record  Cliente(Guid Id, string NomeCompleto);

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