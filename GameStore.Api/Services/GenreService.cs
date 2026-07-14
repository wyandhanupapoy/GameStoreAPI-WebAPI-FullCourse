using GameStore.Api.Dtos;
using GameStore.Api.Models;
using GameStore.Api.Repositories;

namespace GameStore.Api.Services;

public class GenreService(IGenreRepository genreRepository) : IGenreService
{
    public async Task<IEnumerable<GenreDto>> GetAllGenresAsync()
    {
        var genres = await genreRepository.GetAllAsync();
        return genres.Select(genre => new GenreDto(genre.Id, genre.Name));
    }

    public async Task<GenreDto?> GetGenreByIdAsync(int id)
    {
        var genre = await genreRepository.GetAsync(g => g.Id == id);
        if (genre is null) return null;

        return new GenreDto(genre.Id, genre.Name);
    }

    public async Task<GenreDto> CreateGenreAsync(CreateGenreDto newGenre)
    {
        Genre genre = new()
        {
            Name = newGenre.Name
        };

        await genreRepository.AddAsync(genre);
        await genreRepository.SaveAsync();

        return new GenreDto(genre.Id, genre.Name);
    }

    public async Task<bool> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre)
    {
        var existingGenre = await genreRepository.GetAsync(g => g.Id == id);
        if (existingGenre is null) return false;

        if (updatedGenre.Name != null) existingGenre.Name = updatedGenre.Name;

        genreRepository.Update(existingGenre);
        await genreRepository.SaveAsync();
        return true;
    }

    public async Task DeleteGenreAsync(int id)
    {
        var existingGenre = await genreRepository.GetAsync(g => g.Id == id);
        if (existingGenre is not null)
        {
            genreRepository.Remove(existingGenre);
            await genreRepository.SaveAsync();
        }
    }
}