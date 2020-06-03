
# Also run:
# dotnet tool restore

if (test-path .\Trl.PegParser.Tests\TestResults) {
    Remove-Item -r -force .\Trl.PegParser.Tests\TestResults
}

if (test-path .\UnitTestCoverageReport) {
    Remove-Item -r -force .\UnitTestCoverageReport
}

dotnet test --collect:"XPlat Code Coverage"
dotnet tool run reportgenerator -reports:.\Trl.PegParser.Tests\TestResults\*\*.xml -targetdir:.\UnitTestCoverageReport -reporttypes:Html
