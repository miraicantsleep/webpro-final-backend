using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pweb_eas.Data;
using pweb_eas.Models;
using pweb_eas.Models.Entities;

namespace pweb_eas.Controllers
{
    [Route("api/transaction")]
    [Authorize]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;

        public TransactionsController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionDto createTransactionDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "user not found"
                });
            }

            var user = await dbContext.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return NotFound(new
                {
                    message = "user not found"
                });
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = createTransactionDto.Name ?? string.Empty,
                Type = createTransactionDto.Type,
                Amount = createTransactionDto.Amount,
                Notes = createTransactionDto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = null
            };

            await dbContext.Transactions.AddAsync(transaction);
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "true",
                message = "success add transaction",
                data = new {
                    id = transaction.Id,
                    owner = user.Id,
                    name = transaction.Name,
                    type = transaction.Type,
                    amount = transaction.Amount,
                    notes = transaction.Notes
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTransaction()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            if (role == "Admin")
            {
                var transactions = await dbContext.Transactions.ToListAsync();
                return Ok(new
                {
                    message = "success get all transactions",
                    transactions
                });
            }

            if (role == "User")
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var transactions = await dbContext.Transactions.Where(t => t.UserId.ToString() == userId).ToListAsync();
                return Ok(new
                {
                    status = "true",
                    message = "success get all transactions",
                    data = new {
                        count = transactions.Count,
                        data = transactions.Select(t => new {
                            id = t.Id,
                            owner = t.UserId,
                            name = t.Name,
                            type = t.Type,
                            amount = t.Amount,
                            notes = t.Notes
                        })
                    }
                });
            }

            return Unauthorized(new
            {
                message = "invalid roles",
                role
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            if (role == "Admin")
            {
                var transaction = await dbContext.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound(new
                    {
                        message = "transaction not found"
                    });
                }

                return Ok(new
                {
                    message = "success get transaction",
                    transaction
                });
            }

            if (role == "User")
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId.ToString() == userId);
                if (transaction == null)
                {
                    return NotFound(new
                    {
                        message = "transaction not found"
                    });
                }

                return Ok(new
                {
                    status = "true",
                    message = "success get transaction",
                    data = new {
                        id = transaction.Id,
                        owner = transaction.UserId,
                        name = transaction.Name,
                        type = transaction.Type,
                        amount = transaction.Amount,
                        notes = transaction.Notes
                    }
                });
            }
            return Unauthorized(new
            {
                message = "invalid roles",
                role
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(Guid id, UpdateTransactionDto updateTransactionDto)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            if (role == "Admin")
            {
                var transaction = await dbContext.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound(new
                    {
                        message = "transaction not found"
                    });
                }

                transaction.Name = updateTransactionDto.Name ?? transaction.Name;
                transaction.Type = updateTransactionDto.Type;
                transaction.Amount = updateTransactionDto.Amount;
                transaction.Notes = updateTransactionDto.Notes ?? transaction.Notes;
                transaction.UpdatedAt = DateTime.UtcNow;

                dbContext.Transactions.Update(transaction);
                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "success update transaction",
                    transaction
                });
            }

            if (role == "User")
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId.ToString() == userId);
                if (transaction == null)
                {
                    return NotFound(new
                    {
                        message = "transaction not found"
                    });
                }

                transaction.Name = updateTransactionDto.Name ?? transaction.Name;
                transaction.Type = updateTransactionDto.Type;
                transaction.Amount = updateTransactionDto.Amount;
                transaction.Notes = updateTransactionDto.Notes ?? transaction.Notes;
                transaction.UpdatedAt = DateTime.UtcNow;

                dbContext.Transactions.Update(transaction);
                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    status = "true",
                    message = "success update transaction",
                    data = new {
                        id = transaction.Id,
                        owner = transaction.UserId,
                        name = transaction.Name,
                        type = transaction.Type,
                        amount = transaction.Amount,
                        notes = transaction.Notes
                    }
                });
            }
            return Unauthorized(new
            {
                message = "invalid roles",
                role
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(Guid id)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            if (role == "Admin")
            {
                var transaction = await dbContext.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound(new
                    {
                        message = "transaction not found"
                    });
                }

                dbContext.Transactions.Remove(transaction);
                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "success delete transaction",
                    transaction
                });
            }

            if (role == "User")
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId.ToString() == userId);
                if (transaction == null)
                {
                    return NotFound(new
                    {
                        message = "transaction not found"
                    });
                }

                dbContext.Transactions.Remove(transaction);
                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    status = "true",
                    message = "success delete transaction"
                });
            }
            return Unauthorized(new
            {
                message = "invalid roles",
                role
            });
        }
    }
}