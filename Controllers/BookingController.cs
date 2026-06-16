using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Mw3dy.Data;
using Mw3dy.Models;

namespace Mw3dy.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly int _defaultUserId = 1;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Pass branches to the view
            var branches = _context.Branches.OrderBy(b => b.DistanceKm).ToList();
            ViewBag.Branches = branches;

            // Pass services to the view
            var services = _context.Services.ToList();
            ViewBag.Services = services;

            // Get user info
            var user = _context.Users.FirstOrDefault(u => u.Id == _defaultUserId);
            ViewBag.UserName = user?.Name ?? "Adham Emad";
            ViewBag.UserCity = user?.City ?? string.Empty;
            ViewBag.UserPhone = user?.Phone ?? string.Empty;
            ViewBag.UserAddress = user?.Address ?? string.Empty;

            return View();
        }

        [HttpPost]
        public IActionResult Confirm([FromBody] BookingDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Service) || dto.BranchId <= 0 || string.IsNullOrEmpty(dto.Date) || string.IsNullOrEmpty(dto.Time))
            {
                return Json(new { success = false, message = "Invalid booking details." });
            }

            // Verify branch exists
            var branchExists = _context.Branches.Any(b => b.Id == dto.BranchId);
            if (!branchExists)
            {
                return Json(new { success = false, message = "Selected branch does not exist." });
            }

            // Create appointment
            var appointment = new Appointment
            {
                Service = dto.Service,
                BranchId = dto.BranchId,
                Date = dto.Date,
                Time = dto.Time,
                Notes = dto.Notes,
                Status = "confirmed",
                CreatedAt = DateTime.UtcNow,
                UserId = _defaultUserId
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return Json(new { success = true, appointmentId = appointment.Id });
        }
    }

    public class BookingDto
    {
        public string Service { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
