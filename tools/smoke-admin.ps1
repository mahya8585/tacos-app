$checks = @(
    @{ Name='Root (auth -> redirect)'; Path='/';                            Expect=@(302) },
    @{ Name='Login page';              Path='/Identity/Account/Login';      Expect=@(200) },
    @{ Name='Static asset';            Path='/css/site.css';                Expect=@(200, 404) }
)
& "$PSScriptRoot\smoketest.ps1" -BaseUrl 'http://localhost:5069' -Title 'TacosApp.Admin' -Checks $checks
