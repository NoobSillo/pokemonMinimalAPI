using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IPokemonService, PokemonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/pokemon", async (IPokemonService pokemonService) =>
{
    var pokemon = await pokemonService.GetFavoritePokemonAsync();
    return Results.Ok(pokemon);
})
.WithName("GetFavoritePokemon")
.WithOpenApi();

app.Run();

// Interface for Pokemon service
public interface IPokemonService
{
    Task<PokemonDto> GetFavoritePokemonAsync();
}

// Implementation of the Pokemon service
public class PokemonService : IPokemonService
{
    private readonly HttpClient _httpClient;

    public PokemonService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PokemonDto> GetFavoritePokemonAsync()
    {
        var response = await _httpClient.GetAsync("https://pokeapi.co/api/v2/pokemon/charmander");
        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        var content = await response.Content.ReadAsStringAsync();
        var pokemon = JsonSerializer.Deserialize<PokemonDto>(content, options);

        return pokemon;
    }
}

// DTO for Pokemon
public class PokemonDto
{
    public string Name { get; set; }
    public List<TypeElement> Types { get; set; }
    public Sprites Sprites { get; set; }
    public List<MoveElement> Moves { get; set; }
}

// Type element DTO
public class TypeElement
{
    public TypeDetail Type { get; set; }
}

// Type detail DTO
public class TypeDetail
{
    public string Name { get; set; }
}

// Sprites DTO
public class Sprites
{
    [JsonPropertyName("front_default")]
    public string FrontDefault { get; set; }
}

// Move element DTO
public class MoveElement
{
    public MoveDetail Move { get; set; }
}

// Move detail DTO
public class MoveDetail
{
    public string Name { get; set; }
}

