using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mw3dy.Data;
using Mw3dy.Models;

namespace Mw3dy.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        // Static ID for default user Adham Emad
        private readonly int _defaultUserId = 1;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Verify default user exists (it is seeded, but safety check)
            var user = _context.Users.FirstOrDefault(u => u.Id == _defaultUserId);
            if (user == null)
            {
                // Re-seed if missing
                user = new User { Id = _defaultUserId, Name = "Adham Emad", Email = "adham@mw3dy.com" };
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            // Fetch appointments
            var appointments = _context.Appointments
                .Where(a => a.UserId == _defaultUserId)
                .Include(a => a.Branch)
                .ToList();

            ViewBag.UserName = user.Name;
            ViewBag.UserEmail = user.Email;

            return View(appointments);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id && a.UserId == _defaultUserId);
            if (appointment != null)
            {
                appointment.Status = "cancelled";
                _context.SaveChanges();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                TempData["Message"] = "bookingCancelled";
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            return NotFound();
        }
    }
}
