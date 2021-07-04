﻿using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorWasmApp.Common;
using BlazorWasmApp.Services;
using BlazorWasmApp.ViewModels.IdentityModels;
using Microsoft.AspNetCore.Components.Forms;
using TanvirArjel.Blazor;
using TanvirArjel.Blazor.Components;

namespace BlazorWasmApp.Components.IdentityComponents
{
    public partial class LoginComponent
    {
        private readonly UserService _userService;
        private readonly HostAuthStateProvider _hostAuthStateProvider;
        private readonly ExceptionLogger _exceptionLogger;

        public LoginComponent(
            UserService userService,
            HostAuthStateProvider hostAuthStateProvider,
            ExceptionLogger exceptionLogger)
        {
            _userService = userService;
            _hostAuthStateProvider = hostAuthStateProvider;
            _exceptionLogger = exceptionLogger;
        }

        private EditContext FormContext { get; set; }

        private LoginModel LoginModel { get; set; } = new LoginModel();

        private CustomValidationMessages ValidationMessages { get; set; }

        private bool IsDisabled { get; set; }

        protected override void OnInitialized()
        {
            FormContext = new EditContext(LoginModel);
            FormContext.SetFieldCssClassProvider(new BootstrapValidationClassProvider());
        }

        private async Task HandleValidSubmit()
        {
            try
            {
                IsDisabled = true;
                HttpResponseMessage httpResponse = await _userService.LoginAsync(LoginModel);

                if (httpResponse.IsSuccessStatusCode)
                {
                    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    string responseString = await httpResponse.Content.ReadAsStringAsync();
                    LoggedInUserInfo loginResponse = JsonSerializer.Deserialize<LoggedInUserInfo>(responseString, jsonSerializerOptions);

                    if (loginResponse != null)
                    {
                        await _hostAuthStateProvider.LogInAsync(loginResponse, "/");
                    }
                }
                else
                {
                    await ValidationMessages.AddAndDisplayAsync(httpResponse);
                    IsDisabled = false;
                }
            }
            catch (Exception exception)
            {
                ValidationMessages.AddAndDisplay(AppErrorMessage.ClientErrorMessage);
                await _exceptionLogger.LogAsync(exception);
            }
        }
    }
}