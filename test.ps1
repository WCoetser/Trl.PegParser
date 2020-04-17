
# Also run:
# dotnet tool restore

if (test-path .\Trs.PegParser.Tests\TestResults) {
    Remove-Item -r -force .\Trs.PegParser.Tests\TestResults
}

if (test-path .\UnitTestCoverageReport) {
    Remove-Item -r -force .\UnitTestCoverageReport
}

dotnet test --collect:"XPlat Code Coverage"
dotnet tool run reportgenerator -reports:.\Trs.PegParser.Tests\TestResults\*\*.xml -targetdir:.\UnitTestCoverageReport -reporttypes:Html
