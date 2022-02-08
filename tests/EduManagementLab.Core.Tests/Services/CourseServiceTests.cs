﻿using System;
using Xunit;
using EduManagementLab.Core.Services;
using EduManagementLab.EfRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EduManagementLab.Core.Entities;
using System.Linq;

namespace EduManagementLab.Core.Tests.Services
{
    public class CourseServiceTests
    {
        private readonly DbContextOptions<DataContext> _contextOptions = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("CourseServiceTest")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

        private readonly DataContext _dataContext;
        private readonly UnitOfWork _unitOfWork;
        private readonly CourseService _courseService;
        DataContext CreateContext() => new DataContext(_contextOptions);
        UnitOfWork CreateUnitOfWork() => new UnitOfWork(_dataContext);
        CourseService CreateCourseService() => new CourseService(_unitOfWork);
        public CourseServiceTests()
        {

            _dataContext = CreateContext();

            _dataContext.Database.EnsureDeleted();
            _dataContext.Database.EnsureCreated();

            Course course1 = new Course { Id = Guid.Parse("4E228873-0468-4BE6-A14B-48DE5E7CFFFB"), Code = "AAA", Name = "CourseNameOne", Description = "CourseDescriptionOne", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue };
            Course course2 = new Course { Id = Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D"), Code = "BBB", Name = "CourseNameTwo", Description = "CourseDescriptionTwo", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue };

            User user1 = new User { Id = Guid.Parse("8E7A4A48-9FFE-4E66-8AF5-65B7860CFEC0"), Displayname = "DisplayNameOne", Email = "EmailOne@Test.com", FirstName = "FirstNameOne", LastName = "LastNameOne" };

            Course.Membership membership1 = new Course.Membership { Id = Guid.Parse("9C1BA350-62EC-4A90-BD85-647CD15159ED"), Course = course2, CourseId = course2.Id, User = user1, UserId = user1.Id, EnrolledDate = DateTime.MinValue };

            _dataContext.AddRange(
                course1, course2,
                user1);

            _dataContext.SaveChanges();

            _unitOfWork = CreateUnitOfWork();
            _courseService = CreateCourseService();
        }

        [Fact]
        public void GetCourse_ReturnsCorrectCourse()
        {
            var fetchedCourse = _courseService.GetCourse(Guid.Parse("4E228873-0468-4BE6-A14B-48DE5E7CFFFB"));

            Assert.NotNull(fetchedCourse);
            Assert.Equal("AAA", fetchedCourse.Code);
            Assert.Equal("CourseNameOne", fetchedCourse.Name);
            Assert.Equal("CourseDescriptionOne", fetchedCourse.Description);
            Assert.Equal(DateTime.MinValue, fetchedCourse.StartDate);
            Assert.Equal(DateTime.MaxValue, fetchedCourse.EndDate);
        }

        [Fact]
        public void GetAllCourses_ReturnsCorrectCourses()
        {
            var fetchedCourses = _courseService.GetCourses();

            Assert.Collection(
                fetchedCourses,
                b => Assert.Equal("AAA", b.Code),
                b => Assert.Equal("BBB", b.Code));
        }

        [Fact]
        public void AddCourse_ReturnsCorrectCourse()
        {
            var createdCourse = _courseService.CreateCourse("CCC", "CourseNameThree", "CourseDescriptionThree", DateTime.Today, DateTime.Today);

            var course = _dataContext.Courses.Single(b => b.Code == "CCC");
            Assert.Equal("CCC", course.Code);
        }

        [Fact]
        public void UpdateCourseInfo_ReturnsCorrectCourse()
        {
            _courseService.UpdateCourseInfo(Guid.Parse("4E228873-0468-4BE6-A14B-48DE5E7CFFFB"), "AAB", "ChangedCourseName", "ChangedCourseDescription");

            var course = _dataContext.Courses.Single(b => b.Id == Guid.Parse("4E228873-0468-4BE6-A14B-48DE5E7CFFFB"));

            Assert.NotNull(course);
            Assert.Equal("AAB", course.Code);
            Assert.Equal("ChangedCourseName", course.Name);
            Assert.Equal("ChangedCourseDescription", course.Description);
        }

        [Fact]
        public void UpdateCourseEmail_ReturnsCorrectCourse()
        {
            _courseService.UpdateCoursePeriod(Guid.Parse("4E228873-0468-4BE6-A14B-48DE5E7CFFFB"), DateTime.Today, DateTime.Today);

            var course = _dataContext.Courses.Single(b => b.Id == Guid.Parse("4E228873-0468-4BE6-A14B-48DE5E7CFFFB"));

            Assert.NotNull(course);
            Assert.Equal(DateTime.Today, course.StartDate);
            Assert.Equal(DateTime.Today, course.EndDate);
        }

        [Fact]
        public void DeleteCourse_DeletesCorrectCourse()
        {
            _courseService.DeleteCourse(Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D"));

            var fetchedCourses = _courseService.GetCourses();

            Assert.Equal(1 ,fetchedCourses.Count());
        }

        [Fact]
        public void CreateCourseMembership_ReturnsCorrectCourse()
        {
            var testCourseId = Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D");
            var testUserId = Guid.Parse("8E7A4A48-9FFE-4E66-8AF5-65B7860CFEC0");
            var testEnrollmentDate = DateTime.MinValue;

            var course = _courseService.CreateCourseMembership(testCourseId, testUserId, testEnrollmentDate);

            Assert.Equal(Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D"), course.Id);
        }

        [Fact]
        public void RemoveCourseMembership_ReturnsCorrectCourse()
        {
            var testCourseId = Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D");
            var testUserId = Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D");
           
            var course = _courseService.RemoveCourseMembership(testCourseId, testUserId);

            Assert.Equal(Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D"), course.Id);
        }

        [Fact]
        public void UpdateMembershipEnrolledDate_ReturnsCorrectCourse()
        {
            var testCourseId = Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D");
            var testUserId = Guid.Parse("8E7A4A48-9FFE-4E66-8AF5-65B7860CFEC0");
            var testEnrollmentDate = DateTime.MinValue;

            var course = _courseService.CreateCourseMembership(testCourseId, testUserId, testEnrollmentDate);

            Assert.Equal(Guid.Parse("4A0E4335-08E0-45C0-8A97-9791CE81E73D"), course.Id);
        }

    }
}
