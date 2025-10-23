using MongoDB.Driver;
using CompanyService.Data;
using CompanyService.Models;

namespace CompanyService.Services
{
    public class CompanyDataService
    {
        private readonly MongoDbContext _context;

        public CompanyDataService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllCompanies()
            => await _context.Companies.Find(_ => true).ToListAsync();

        public async Task<Company?> GetCompanyById(string id)
            => await _context.Companies.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task CreateCompany(Company company)
            => await _context.Companies.InsertOneAsync(company);

        public async Task<bool> UpdateCompany(string id, Company company)
        {
            var result = await _context.Companies.ReplaceOneAsync(c => c.Id == id, company);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteCompany(string id)
        {
            var result = await _context.Companies.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
