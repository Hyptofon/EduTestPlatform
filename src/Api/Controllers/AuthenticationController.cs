using Api.Controllers;
using Api.Dtos;
using Application.Authentication.Commands.Login;
using Application.Authentication.Commands.RefreshToken;
using Application.Authentication.Commands.Register;
using Application.Authentication.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class AuthenticationController(ISender sender) : ApiController(sender)
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            InviteCode = request.InviteCode
        };

        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous] // Можна зробити AllowAnonymous, бо токен може бути вже прострочений
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            Token = request.Token,
            RefreshToken = request.RefreshToken
        };

        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}

