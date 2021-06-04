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
        public ActionResult Index()
        {
            var leaveTypes = _leavetyperepo.FindAll().ToList();
            var mappedLeaveTypeVM = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes);
            var model = new CreateLeaveAllocationVM
            {
                NumberUpdated = 0,
                LeaveTypes = mappedLeaveTypeVM
            };

            return View(model);
        }

        public ActionResult SetLeave(int id)
        {
            var leaveType = _leavetyperepo.FindById(id);
            var employees = _userManager.GetUsersInRoleAsync("Employee").Result;

            foreach (var emp in employees)
            {
                if (_leaveallocationrepo.CheckAllocation(id, emp.Id))
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
                _leaveallocationrepo.Create(leaveAllocation);
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult ListEmployees()
        {
            var employees = _userManager.GetUsersInRoleAsync("Employee").Result;
            var model = _mapper.Map<List<EmployeeVM>>(employees); //because return list of employees
            return View(model);
        }

        // GET: LeaveAllocationController/Details/5
        public ActionResult Details(string id)
        {
            var employee = _userManager.FindByIdAsync(id).Result; //get employee data
            var employeemap = _mapper.Map<EmployeeVM>(employee); // map employee to employee vm

            var leaveallocation = _leaveallocationrepo.GetLeaveAllocationsByEmployee(id); //get leave allocation of employee
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
        public ActionResult Edit(int id)
        {
            var leaveallocation = _leaveallocationrepo.FindById(id);
            var model = _mapper.Map<EditLeaveAllocationVM>(leaveallocation);
            model.LeaveTypes = _mapper.Map<LeaveTypeVM>(_leavetyperepo.FindById(model.Id));
            model.Employee = _mapper.Map<EmployeeVM>(_userManager.FindByIdAsync(model.EmployeeId).Result);
            return View(model);
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditLeaveAllocationVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    AddModelError("");
                    return View(model);
                }

                // This is a local method, visible ONLY inside the Edit action. The variable "vm" already exists in the enclosing action.
                void AddModelError(string propertyName)
                {
                    model.LeaveTypes = _mapper.Map<LeaveTypeVM>(_leavetyperepo.FindById(model.Id));
                    model.Employee = _mapper.Map<EmployeeVM>(_userManager.FindByIdAsync(model.EmployeeId).Result);
                    ModelState.AddModelError(propertyName, "Something went wrong...");
                }

                var record = _leaveallocationrepo.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;
                var isSuccess = _leaveallocationrepo.Update(record);
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
