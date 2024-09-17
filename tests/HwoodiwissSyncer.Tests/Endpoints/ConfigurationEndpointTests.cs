﻿using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using HwoodiwissSyncer.Tests.Integration.Assertions;

namespace HwoodiwissSyncer.Tests.Integration.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class ConfigurationEndpointTests(IntegrationFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Get_Version_ReturnsApplicationMetadata()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/configuration/version");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var actualContent = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonNode>>();
        actualContent.ShouldNotBeNull();
        actualContent.Keys.ShouldContainAll([
            "version",
            "gitBranch",
            "gitCommit",
            "systemArchitecture",
            "runtimeVersion",
            "aspNetCoreVersion",
            "aspNetCoreRuntimeVersion",
            "isDynamicCodeCompiled",
            "isDynamicCodeSupported",
            "isNativeAot",
        ]);
    }
}
