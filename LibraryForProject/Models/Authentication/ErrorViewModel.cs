﻿using System.ComponentModel.DataAnnotations;
namespace LibraryForProject.Models.Authentication
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
