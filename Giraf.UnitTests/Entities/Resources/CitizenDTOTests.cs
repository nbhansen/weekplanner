using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using GirafAPI.Entities.Citizens.DTOs;
using Xunit;

namespace GirafAPI.UnitTests.Entities.Resources
{
    public class CitizenDTOTests
    {
        [Fact]
        public void CitizenDTO_ValidData_SetsProperties()
        {
            var citizen = new CitizenDTO(1, "John", "Doe");

            Assert.Equal(1, citizen.Id);
            Assert.Equal("John", citizen.FirstName);
            Assert.Equal("Doe", citizen.LastName);
        }

        [Fact]
        public void CitizenDTO_WithEmptyStrings_StillConstructs()
        {
            var citizen = new CitizenDTO(0, "", "");

            Assert.Equal(0, citizen.Id);
            Assert.Equal("", citizen.FirstName);
            Assert.Equal("", citizen.LastName);
        }

        [Fact]
        public void CitizenDTO_EqualityByValue()
        {
            var a = new CitizenDTO(1, "John", "Doe");
            var b = new CitizenDTO(1, "John", "Doe");
            var c = new CitizenDTO(2, "Jane", "Doe");

            Assert.Equal(a, b);
            Assert.NotEqual(a, c);
        }
    }
}