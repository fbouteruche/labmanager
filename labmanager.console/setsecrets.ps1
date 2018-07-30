$organization = Read-Host "Organization URI"
$pat = Read-Host "Personnal Access Token"
$project = Read-Host "Project"
dotnet user-secrets set organization $organization
dotnet user-secrets set pat $pat
dotnet user-secrets set project $project