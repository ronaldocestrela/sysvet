#!/bin/bash
set -e

SLN="SaaS_Veterinario.slnx"
rm -f SysVet.slnx

# Function to create module projects
create_module() {
    local mod=$1
    echo "Creating module $mod..."
    
    # Domain
    dotnet new classlib -n "$mod.Domain" -o "src/Modules/$mod/Domain" -f net10.0 --force
    rm -f "src/Modules/$mod/Domain/Class1.cs"
    dotnet add "src/Modules/$mod/Domain/$mod.Domain.csproj" reference src/Modules/Core/Domain/Core.Domain.csproj
    
    # Application
    dotnet new classlib -n "$mod.Application" -o "src/Modules/$mod/Application" -f net10.0 --force
    rm -f "src/Modules/$mod/Application/Class1.cs"
    dotnet add "src/Modules/$mod/Application/$mod.Application.csproj" reference "src/Modules/$mod/Domain/$mod.Domain.csproj"
    dotnet add "src/Modules/$mod/Application/$mod.Application.csproj" reference src/Modules/Core/Application/Core.Application.csproj
    
    # Infrastructure
    dotnet new classlib -n "$mod.Infrastructure" -o "src/Modules/$mod/Infrastructure" -f net10.0 --force
    rm -f "src/Modules/$mod/Infrastructure/Class1.cs"
    dotnet add "src/Modules/$mod/Infrastructure/$mod.Infrastructure.csproj" reference "src/Modules/$mod/Application/$mod.Application.csproj"
    dotnet add "src/Modules/$mod/Infrastructure/$mod.Infrastructure.csproj" reference src/Modules/Core/Infrastructure/Core.Infrastructure.csproj
    
    # Add to API
    dotnet add src/API/API.csproj reference "src/Modules/$mod/Application/$mod.Application.csproj"
    dotnet add src/API/API.csproj reference "src/Modules/$mod/Infrastructure/$mod.Infrastructure.csproj"
    
    # Add to solution
    dotnet sln "$SLN" add "src/Modules/$mod/Domain/$mod.Domain.csproj" --solution-folder "src/Modules/$mod/Domain"
    dotnet sln "$SLN" add "src/Modules/$mod/Application/$mod.Application.csproj" --solution-folder "src/Modules/$mod/Application"
    dotnet sln "$SLN" add "src/Modules/$mod/Infrastructure/$mod.Infrastructure.csproj" --solution-folder "src/Modules/$mod/Infrastructure"
    
    # Tests
    dotnet new xunit -n "$mod.Tests" -o "tests/Modules/$mod.Tests" -f net10.0 --force
    dotnet add "tests/Modules/$mod.Tests/$mod.Tests.csproj" reference "src/Modules/$mod/Application/$mod.Application.csproj"
    dotnet add "tests/Modules/$mod.Tests/$mod.Tests.csproj" reference "src/Modules/$mod/Domain/$mod.Domain.csproj"
    dotnet sln "$SLN" add "tests/Modules/$mod.Tests/$mod.Tests.csproj" --solution-folder "tests/Modules"
}

create_module "Veterinary"
create_module "Petshop"
create_module "Sales"
create_module "Fiscal"
create_module "Inventory"

echo "Creating Clients..."
# SharedUI
dotnet new razorclasslib -n SharedUI -o src/Clients/SharedUI -f net10.0 --force
dotnet sln "$SLN" add src/Clients/SharedUI/SharedUI.csproj --solution-folder "src/Clients"

# BlazorWeb
dotnet new blazorwasm -n BlazorWeb -o src/Clients/BlazorWeb -f net10.0 --force
dotnet add src/Clients/BlazorWeb/BlazorWeb.csproj reference src/Clients/SharedUI/SharedUI.csproj
dotnet sln "$SLN" add src/Clients/BlazorWeb/BlazorWeb.csproj --solution-folder "src/Clients"

# MauiApp
dotnet new maui-blazor -n MauiApp -o src/Clients/MauiApp -f net10.0 --force || true
if [ -f src/Clients/MauiApp/MauiApp.csproj ]; then
    dotnet add src/Clients/MauiApp/MauiApp.csproj reference src/Clients/SharedUI/SharedUI.csproj
    dotnet sln "$SLN" add src/Clients/MauiApp/MauiApp.csproj --solution-folder "src/Clients"
else
    echo "Could not create MauiApp, template might not be available."
fi

# API Tests
echo "Creating API Integration Tests..."
dotnet new xunit -n API.IntegrationTests -o tests/API.IntegrationTests -f net10.0 --force
dotnet add tests/API.IntegrationTests/API.IntegrationTests.csproj reference src/API/API.csproj
dotnet sln "$SLN" add tests/API.IntegrationTests/API.IntegrationTests.csproj --solution-folder "tests"

# Client Tests
echo "Creating Client Tests..."
dotnet new xunit -n Clients.Tests -o tests/Clients.Tests -f net10.0 --force
dotnet add tests/Clients.Tests/Clients.Tests.csproj reference src/Clients/SharedUI/SharedUI.csproj
dotnet sln "$SLN" add tests/Clients.Tests/Clients.Tests.csproj --solution-folder "tests"

echo "Building Solution..."
dotnet build "$SLN"
