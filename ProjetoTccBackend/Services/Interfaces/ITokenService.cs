using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified user.
        /// </summary>
        /// <param name="user">The user for whom the token is being generated.</param>
        /// <param name="role">The role of the user.</param>
        /// <returns>A string representation of the generated JWT.</returns>
        /// <remarks>
        /// The token includes claims such as the user's email, ID, role, and a unique identifier (JTI).
        /// It is signed using a symmetric security key and is valid for 5 days.
        /// </remarks>
        public string GenerateUserToken(User user, string role);


        /// <summary>
        /// Generates a JSON Web Token (JWT) that serves as an invitation for a user to assume the "Teacher" role.
        /// </summary>
        /// <param name="expiration">The duration after which the token will expire.</param>
        /// <returns>A string representation of the generated JWT containing the "Teacher" role and an invite claim.</returns>
        /// <remarks>
        /// The token includes claims indicating an invitation and the "Teacher" role, as well as a unique identifier (JTI).
        /// It is signed using a symmetric security key and is valid for the specified expiration period.
        /// </remarks>
        public string GenerateTeacherRoleInviteToken(TimeSpan expiration);


        /// <summary>
        /// Validates a JSON Web Token (JWT) against the configured issuer, audience, and security key.
        /// </summary>
        /// <param name="token">The JWT token to be validated.</param>
        /// <returns>True if the token is valid, false otherwise.</returns>
        /// <remarks>
        /// This method checks the token's issuer, audience, and signature, as well as its lifetime.
        /// If any of these checks fail, the method returns false.
        /// </remarks>
        bool ValidateToken(string token);
    }
}
