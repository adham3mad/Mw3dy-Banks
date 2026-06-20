using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mw3dy.Data;
using Mw3dy.Models;
using Mw3dy.Services;

namespace Mw3dy.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly LocalizationService _localizer;

        private int CurrentUserId
        {
            get
            {
                if (Request.Cookies.TryGetValue("mw3dy-user-id", out var idStr) && int.TryParse(idStr, out var id))
                {
                    return id;
                }
                return 1;
            }
        }

        public EmployeeController(AppDbContext context, LocalizationService localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            // Check if current user is employee
            var currentUser = _context.Users.Include(u => u.Branch).FirstOrDefault(u => u.Id == CurrentUserId);
            if (currentUser == null || !currentUser.IsEmployee)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Load branches for filter dropdown. If employee is tied to a branch, they can only filter that branch.
            var branches = _context.Branches.OrderBy(b => b.NameEn).AsQueryable();
            if (currentUser.BranchId.HasValue)
            {
                branches = branches.Where(b => b.Id == currentUser.BranchId.Value);
            }
            ViewBag.Branches = branches.ToList();

            // Fetch appointments. Filter by employee's branch if configured.
            var query = _context.Appointments
                .Include(a => a.Branch)
                .Include(a => a.User)
                .AsQueryable();

            if (currentUser.BranchId.HasValue)
            {
                query = query.Where(a => a.BranchId == currentUser.BranchId.Value);
                ViewBag.EmployeeBranchId = currentUser.BranchId.Value;
                ViewBag.EmployeeBranchName = _localizer.GetCurrentCulture() == "ar" ? currentUser.Branch?.NameAr : currentUser.Branch?.NameEn;
            }

            var appointments = query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            // Calculate statistics
            ViewBag.TotalCount = appointments.Count;
            ViewBag.PendingCount = appointments.Count(a => a.Status == "confirmed");
            ViewBag.CompletedCount = appointments.Count(a => a.Status == "completed");
            ViewBag.CancelledCount = appointments.Count(a => a.Status == "cancelled");

            return View(appointments);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status, string? remarks)
        {
            // Check if current user is employee
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == CurrentUserId);
            if (currentUser == null || !currentUser.IsEmployee)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            if (string.IsNullOrEmpty(status))
            {
                return Json(new { success = false, message = "Status is required." });
            }

            // Normalize status to lowercase
            status = status.ToLower();
            if (status != "confirmed" && status != "completed" && status != "cancelled")
            {
                return Json(new { success = false, message = "Invalid status value." });
            }

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found." });
            }

            // Verify if the employee is restricted to a branch and the appointment belongs to it
            if (currentUser.BranchId.HasValue && appointment.BranchId != currentUser.BranchId.Value)
            {
                return Json(new { success = false, message = "Unauthorized to manage appointments for other branches." });
            }

            appointment.Status = status;
            appointment.EmployeeRemarks = remarks;
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
