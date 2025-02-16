﻿using Microsoft.AspNetCore.Components;
using Recruit.Client.Extensions;
using Recruit.Client.Pages.ApplicantPages;
using Recruit.Shared;
using System.Net.Http.Json;

namespace Recruit.Client.Pages
{
    public partial class Applicants
    {
        [Inject] HttpClient? Http { get; set; }

        private List<Applicant>? applicants;
        private List<Applicant>? filteredApplicants;
        private int? CurrentApplicantId { get; set; }
        private Applicant? selectedApplicant { get; set; }

        public List<Job?> Positions { get; set; } = new();

        private bool ShowDeleteDialog = false;
        private bool ShowCopyDialog = false;
        private bool ShowMoveDialog = false;

        protected override async Task OnInitializedAsync()
        {
            applicants = await Http!.GetFromJsonAsync<List<Applicant>>("api/Applicants");
            if (applicants?.Count > 0)
                Positions = applicants.Select(a => a.Job).DistinctBy(j => j?.Id).ToList();

            filteredApplicants = applicants;
        }

        public void ShowDetails(int? id)
        {
            CurrentApplicantId = id;
        }

        public void HideDetails()
        {
            CurrentApplicantId = null;
        }

        private void OpenDeleteDialog(Applicant applicant)
        {
            ShowDeleteDialog = true;
            selectedApplicant = applicant;
            StateHasChanged();
        }

        private void OpenCopyDialog(Applicant applicant)
        {
            selectedApplicant = applicant;
            ShowCopyDialog = true;
            StateHasChanged();
        }

        private void OpenMoveDialog(Applicant applicant)
        {
            selectedApplicant = applicant;
            ShowMoveDialog = true;
            StateHasChanged();
        }

        private async Task OnDeleteDialogClose(bool confirmed)
        {
            if (confirmed && selectedApplicant != null)
            {
                var response = await Http!.DeleteAsync($"api/Applicants/{selectedApplicant.Id}");
                if (response.IsSuccessStatusCode)
                {
                    applicants?.Remove(selectedApplicant);
                }
            }

            ShowDeleteDialog = false;
            StateHasChanged();
        }

        private void HandleStageUpdate(Applicant applicant)
        {
            var applicantToUpdate = applicants?.FirstOrDefault(a => a.Id == applicant.Id);
            if (applicantToUpdate != null)
            {
                applicantToUpdate.Stage = applicant.Stage;
                StateHasChanged();
            }
        }


        private void HandleCancel()
        {
            selectedApplicant = null;
            ShowCopyDialog = false;
            ShowMoveDialog = false;
            ShowDeleteDialog = false;
        }

        private void HandleCopy(Applicant applicant)
        {
            int index = applicants?.FindIndex(a => a.Id == selectedApplicant?.Id) ?? 0;
            applicants?.Insert(index + 1, applicant);
            ShowCopyDialog = false;
            StateHasChanged();
        }

        private void HandleMove(Applicant applicant)
        {
            applicants?.Replace(selectedApplicant!, applicant);
            ShowMoveDialog = false;
            StateHasChanged();
        }

        private void FitlerByPosition(ChangeEventArgs args)
        {
            if (int.TryParse(args.Value?.ToString(), out int jobId))
            {
                filteredApplicants = jobId > 0 ?
                    applicants?.Where(a => a.Job?.Id == jobId).ToList() : applicants;
            }
        }
    }
}
