$organization = Read-Host "Organization URI"
$pat = Read-Host "Personnal Access Token"
$project = Read-Host "Project"
$release = Read-Host "Release Name"
dotnet user-secrets set organization $organization
dotnet user-secrets set pat $pat
dotnet user-secrets set project $project
dotnet user-secrets set release $release