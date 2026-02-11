using System.ComponentModel.DataAnnotations;
using Xunit;
using System.Collections.Generic;
using GirafAPI.Entities.Users.DTOs;

namespace GirafAPI.UnitTests.Entities.Users
{
    public class CreateUserDTOTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void Email_IsRequired()
        {
            // Arrange
            var newUser = new CreateUserDTO
            {
                FirstName = "User",
                LastName = "Userson",
                Email = null!,
                Password = "validpassword"
            };            

            // Act
            var validationResults = ValidateModel(newUser);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.Email)));
        }

        [Fact]
        public void Password_IsRequired()
        {
            // Arrange
            var newUser = new CreateUserDTO
            {
                FirstName = "User",
                LastName = "Userson",
                Email = "ValidEmail@email.com", 
                Password = null!
            };            

            // Act
            var validationResults = ValidateModel(newUser);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.Password)));
        }

        [Fact]
        public void Email_CannotExceedMaxLength()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                FirstName = "User",
                LastName = "Userson",
                Email = new string('a', 51), // Exceeds max length
                Password = "validpassword"
            };

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.Email)));
        }

        [Fact]
        public void Password_CannotExceedMaxLength()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                FirstName = "User",
                LastName = "Userson",
                Email = "validusername",
                Password = new string('a', 101) // Exceeds max length
            };

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.Password)));
        }

        [Fact]
        public void ValidCreateUserDTO_PassesValidation()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                FirstName = "User",
                LastName = "Userson",
                Email = "validusername",
                Password = "validpassword"
            };

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Assert
            Assert.Empty(validationResults);
        }
    }
}