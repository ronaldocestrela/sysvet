using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veterinary.Application.Clinical.Dtos;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using System.Text.Json;
using Xunit;

namespace API.IntegrationTests.Veterinary;

[Collection("IntegrationTests")]
public class ClinicalEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly WebApplicationFactory<Program> _factory;

    public ClinicalEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task UploadDownloadAttachment_ForEligibleAppointment_SucceedsForAuthorizedUser()
    {
        var client = await CreateAuthenticatedClientAsync();
        var appointmentId = await SeedEligibleAppointmentAsync();

        var content = new MultipartFormDataContent();
        var bytes = Encoding.UTF8.GetBytes("%PDF-1.4 test");
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "exam.pdf");

        var uploadResponse = await client.PostAsync($"/api/v1/appointments/{appointmentId}/attachments", content);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();
        uploadResponse.IsSuccessStatusCode.Should().BeTrue($"upload failed: {uploadResponse.StatusCode} {uploadBody}");
        var attachmentId = Guid.Parse(uploadBody.Trim('"'));

        var metaResponse = await client.GetAsync($"/api/v1/attachments/{attachmentId}");
        metaResponse.IsSuccessStatusCode.Should().BeTrue();
        var meta = await metaResponse.Content.ReadFromJsonAsync<ClinicalAttachmentDto>(JsonOptions);
        meta!.AppointmentId.Should().Be(appointmentId);

        var downloadResponse = await client.GetAsync($"/api/v1/attachments/{attachmentId}/content");
        downloadResponse.IsSuccessStatusCode.Should().BeTrue();
        downloadResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task DownloadAttachment_WithoutToken_ReturnsUnauthorized()
    {
        var client = await CreateAuthenticatedClientAsync();
        var appointmentId = await SeedEligibleAppointmentAsync();

        var content = new MultipartFormDataContent();
        var anonymousFile = new ByteArrayContent("%PDF-1.4"u8.ToArray());
        anonymousFile.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(anonymousFile, "file", "exam.pdf");
        var uploadResponse = await client.PostAsync($"/api/v1/appointments/{appointmentId}/attachments", content);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();
        uploadResponse.IsSuccessStatusCode.Should().BeTrue();
        var attachmentId = Guid.Parse(uploadBody.Trim('"'));

        var anonymous = _factory.CreateClient();
        var downloadResponse = await anonymous.GetAsync($"/api/v1/attachments/{attachmentId}/content");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Exam_RequestAndComplete_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();
        var appointmentId = await SeedEligibleAppointmentAsync();

        var requestResponse = await client.PostAsJsonAsync($"/api/v1/appointments/{appointmentId}/exams", new { Name = "Hemogram", Category = "Laboratory" });
        requestResponse.IsSuccessStatusCode.Should().BeTrue();
        var examId = await ReadGuidAsync(requestResponse.Content);

        var completeResponse = await client.PostAsJsonAsync($"/api/v1/exams/{examId}/complete", new { ResultSummary = "Within range" });
        completeResponse.IsSuccessStatusCode.Should().BeTrue();

        var listResponse = await client.GetAsync($"/api/v1/appointments/{appointmentId}/exams");
        var listBody = await listResponse.Content.ReadAsStringAsync();
        listResponse.IsSuccessStatusCode.Should().BeTrue(listBody);
        var exams = JsonSerializer.Deserialize<List<ClinicalExamDto>>(listBody, JsonOptions);
        exams!.Should().ContainSingle(e => e.Id == examId && e.Status == "Completed");
    }

    [Fact]
    public async Task Prescription_CreateAndIssue_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();
        var appointmentId = await SeedEligibleAppointmentAsync();

        var createResponse = await client.PostAsJsonAsync($"/api/v1/appointments/{appointmentId}/prescriptions", new { TemplateId = (Guid?)null });
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var prescriptionId = await ReadGuidAsync(createResponse.Content);

        var itemsResponse = await client.PutAsJsonAsync($"/api/v1/prescriptions/{prescriptionId}/items", new
        {
            Items = new[]
            {
                new { Id = Guid.Empty, MedicationName = "Dipirona", Concentration = "500mg", Dose = "1 comp", Route = "PO", Frequency = "8/8h", Duration = "3d", Instructions = "", SortOrder = 0 }
            }
        });
        var itemsBody = await itemsResponse.Content.ReadAsStringAsync();
        itemsResponse.IsSuccessStatusCode.Should().BeTrue(itemsBody);

        var issueResponse = await client.PostAsync($"/api/v1/prescriptions/{prescriptionId}/issue", null);
        issueResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    private async Task<Guid> SeedEligibleAppointmentAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Check").Value;
        appointment.Confirm();
        appointment.Start();
        vetContext.Appointments.Add(appointment);
        await vetContext.SaveChangesAsync();
        return appointmentId;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var coreContext = scope.ServiceProvider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();

        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        await vetContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Core.Infrastructure.Identity.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
        }

        var user = await userManager.FindByEmailAsync("vet-clinical@sysvet.com");
        if (user is null)
        {
            user = new Core.Infrastructure.Identity.AppUser { UserName = "vet-clinical@sysvet.com", Email = "vet-clinical@sysvet.com", TenantId = Guid.NewGuid() };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var httpClient = _factory.CreateClient();
        var loginResponse = await httpClient.PostAsJsonAsync("/api/v1/auth/login", new { Email = "vet-clinical@sysvet.com", Password = "Password123!" });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return httpClient;
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);

    private static async Task<Guid> ReadGuidAsync(HttpContent content)
    {
        var body = await content.ReadAsStringAsync();
        return Guid.Parse(body.Trim('"'));
    }
}
