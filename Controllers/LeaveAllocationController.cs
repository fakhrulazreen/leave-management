using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveAllocationRepository _leaveallocationrepo;
        private readonly ILeaveTypeRepository _leavetyperepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(ILeaveAllocationRepository leaveallocationrepo, ILeaveTypeRepository leavetyperepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _leaveallocationrepo = leaveallocationrepo;
            _leavetyperepo = leavetyperepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveAllocationController
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _leavetyperepo.FindAll();
            var mappedLeaveTypeVM = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes.ToList());
            var model = new CreateLeaveAllocationVM
            {
                NumberUpdated = 0,
                LeaveTypes = mappedLeaveTypeVM
            };

            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType = await _leavetyperepo.FindById(id);
            var employees = await _userManager.GetUsersInRoleAsync("Employee");

            foreach (var emp in employees)
            {
                if (await _leaveallocationrepo.CheckAllocation(id, emp.Id))
                    continue;

                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Now.Year
                };

                var leaveAllocation = _mapper.Map<LeaveAllocation>(allocation);
                await _leaveallocationrepo.Create(leaveAllocation);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ListEmployees()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _mapper.Map<List<EmployeeVM>>(employees); //because return list of employees
            return View(model);
        }

        // GET: LeaveAllocationController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee = await _userManager.FindByIdAsync(id); //get employee data
            var employeemap = _mapper.Map<EmployeeVM>(employee); // map employee to employee vm

            var leaveallocation = await _leaveallocationrepo.GetLeaveAllocationsByEmployee(id); //get leave allocation of employee
            var leaveallocationmap = _mapper.Map<List<LeaveAllocationVM>>(leaveallocation); // map leave allocation to list<leave allocation vm>

            var model = new ViewAllocationVM
            {
                Employee = employeemap,
                EmployeeId = id,
                LeaveAllocations = leaveallocationmap
            }; // return view
            return View(model);
        }

        // GET: LeaveAllocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveallocation = await _leaveallocationrepo.FindById(id);
            var model = _mapper.Map<EditLeaveAllocationVM>(leaveallocation);
            model.LeaveTypes = _mapper.Map<LeaveTypeVM>(await _leavetyperepo.FindById(model.Id));
            model.Employee = _mapper.Map<EmployeeVM>(await _userManager.FindByIdAsync(model.EmployeeId));
            return View(model);
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await AddModelError("");
                    return View(model);
                }

                // This is a local method, visible ONLY inside the Edit action. The variable "vm" already exists in the enclosing action.
                async Task AddModelError(string propertyName)
                {
                    var leaveType = await _leavetyperepo.FindById(model.Id);
                    model.LeaveTypes = _mapper.Map<LeaveTypeVM>(leaveType);
                    var employee = await _userManager.FindByIdAsync(model.EmployeeId);
                    model.Employee = _mapper.Map<EmployeeVM>(employee);
                    ModelState.AddModelError(propertyName, "Something went wrong...");
                }

                var record = await _leaveallocationrepo.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;
                var isSuccess = await _leaveallocationrepo.Update(record);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Error while saving");
                    return View(model);
                }


                return RedirectToAction(nameof(Details),new {id = model.EmployeeId }); // return to Details function
            }
            catch
            {
                return View(model);
            }
        }

        // GET: LeaveAllocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
