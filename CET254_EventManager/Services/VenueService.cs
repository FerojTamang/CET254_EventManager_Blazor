using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CET254_EventManager.Services
{
    // Service layer for venue management and delete protection.
    public class VenueService : IVenueService
    {
        private readonly IRepository<Venue> _venueRepository;

        public VenueService(IRepository<Venue> venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<List<Venue>> GetAllVenuesAsync()
        {
            return await _venueRepository.Query()
                .OrderBy(venue => venue.Name)
                .ToListAsync();
        }

        public async Task<Venue> GetVenueByIdAsync(int venueId)
        {
            return await _venueRepository.GetByIdAsync(venueId)
                ?? throw new VenueNotFoundException(venueId);
        }

        public async Task<Venue> CreateVenueAsync(Venue venue)
        {
            if (!venue.Validate())
            {
                throw new AppException("Venue details are invalid.", "INVALID_VENUE");
            }

            await _venueRepository.AddAsync(venue);
            await _venueRepository.SaveChangesAsync();

            return venue;
        }

        public async Task UpdateVenueAsync(Venue venue)
        {
            if (!await _venueRepository.ExistsAsync(venue.Id))
            {
                throw new VenueNotFoundException(venue.Id);
            }

            if (!venue.Validate())
            {
                throw new AppException("Venue details are invalid.", "INVALID_VENUE");
            }

            _venueRepository.Update(venue);
            await _venueRepository.SaveChangesAsync();
        }

        public async Task DeleteVenueAsync(int venueId)
        {
            var venue = await _venueRepository.Query()
                .Include(v => v.EventVenues)
                .FirstOrDefaultAsync(v => v.Id == venueId)
                ?? throw new VenueNotFoundException(venueId);

            // Delete protection: venues assigned to events cannot be removed.
            if (venue.EventVenues.Any())
            {
                throw new AppException("This venue cannot be deleted because it is assigned to one or more events.", "VENUE_IN_USE");
            }

            _venueRepository.Delete(venue);
            await _venueRepository.SaveChangesAsync();
        }

        public async Task<int> GetTotalVenuesCountAsync()
        {
            return await _venueRepository.CountAsync();
        }
    }
}
