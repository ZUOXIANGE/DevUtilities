using System;
using System.Threading.Tasks;
using Xunit;
using DevUtilities.ViewModels;

namespace DevUtilities.Tests.ViewModels
{
    public class DockerComposeConverterViewModelTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Arrange & Act
            var viewModel = new DockerComposeConverterViewModel();

            // Assert
            Assert.Equal("my-service", viewModel.ServiceName);
            Assert.Empty(viewModel.DockerRunInput);
            Assert.Empty(viewModel.DockerComposeOutput);
            Assert.Empty(viewModel.StatusMessage);
        }

        [Fact]
        public async Task ConvertCommand_WithBasicDockerRun_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("version: '3.8'", output);
            Assert.Contains("services:", output);
            Assert.Contains("my-service:", output);
            Assert.Contains("image: nginx", output);
        }

        [Fact]
        public async Task ConvertCommand_WithPortMapping_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run -p 80:80 nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("ports:", output);
            Assert.Contains("- \"80:80\"", output);
        }

        [Fact]
        public async Task ConvertCommand_WithVolumeMount_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run -v /var/run/docker.sock:/tmp/docker.sock:ro nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("volumes:", output);
            Assert.Contains("- /var/run/docker.sock:/tmp/docker.sock:ro", output);
        }

        [Fact]
        public async Task ConvertCommand_WithRestartPolicy_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run --restart always nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("restart: always", output);
        }

        [Fact]
        public async Task ConvertCommand_WithLogOptions_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run --log-opt max-size=1g nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("logging:", output);
            Assert.Contains("driver: \"json-file\"", output);
            Assert.Contains("options:", output);
            Assert.Contains("max-size: \"1g\"", output);
        }

        [Fact]
        public async Task ConvertCommand_WithComplexCommand_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run -p 80:80 -v /var/run/docker.sock:/tmp/docker.sock:ro --restart always --log-opt max-size=1g nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            
            // 验证基本结构
            Assert.Contains("version: '3.8'", output);
            Assert.Contains("services:", output);
            Assert.Contains("my-service:", output);
            Assert.Contains("image: nginx", output);
            
            // 验证端口映射
            Assert.Contains("ports:", output);
            Assert.Contains("- \"80:80\"", output);
            
            // 验证卷挂载
            Assert.Contains("volumes:", output);
            Assert.Contains("- /var/run/docker.sock:/tmp/docker.sock:ro", output);
            
            // 验证重启策略
            Assert.Contains("restart: always", output);
            
            // 验证日志配置
            Assert.Contains("logging:", output);
            Assert.Contains("driver: \"json-file\"", output);
            Assert.Contains("options:", output);
            Assert.Contains("max-size: \"1g\"", output);
        }

        [Fact]
        public async Task ConvertCommand_WithEnvironmentVariables_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run -e ENV_VAR=value -e ANOTHER_VAR=another nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("environment:", output);
            Assert.Contains("- ENV_VAR=value", output);
            Assert.Contains("- ANOTHER_VAR=another", output);
        }

        [Fact]
        public async Task ConvertCommand_WithContainerName_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run --name my-nginx nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("container_name: my-nginx", output);
        }

        [Fact]
        public async Task ConvertCommand_WithRemoveFlag_GeneratesCorrectCompose()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run --rm nginx";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains("# 原命令使用了 --rm 参数", output);
            Assert.Contains("# docker-compose 中可以使用 'docker-compose down' 来删除容器", output);
        }

        [Fact]
        public async Task ConvertCommand_WithEmptyInput_SetsErrorMessage()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            Assert.Contains("请输入", viewModel.StatusMessage);
        }

        [Fact]
        public async Task ConvertCommand_WithInvalidInput_SetsErrorMessage()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "invalid command";

            // Act
            await viewModel.ConvertCommand();

            // Assert
            Assert.Contains("请输入有效的", viewModel.StatusMessage);
        }

        [Fact]
        public async Task ClearCommand_ClearsAllFields()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "docker run nginx";
            viewModel.DockerComposeOutput = "some output";
            viewModel.StatusMessage = "some status";

            // Act
            await viewModel.ClearCommand();

            // Assert
            Assert.Empty(viewModel.DockerRunInput);
            Assert.Empty(viewModel.DockerComposeOutput);
            Assert.Equal("已清空所有内容", viewModel.StatusMessage);
        }

        [Fact]
        public async Task SwapCommand_SwapsInputAndOutput()
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = "input content";
            viewModel.DockerComposeOutput = "output content";

            // Act
            await viewModel.SwapCommand();

            // Assert
            Assert.Equal("output content", viewModel.DockerRunInput);
            Assert.Equal("input content", viewModel.DockerComposeOutput);
            Assert.Equal("已交换输入输出内容", viewModel.StatusMessage);
        }

        [Theory]
        [InlineData("docker run -p 8080:80 nginx", "8080:80")]
        [InlineData("docker run --publish 3000:3000 node", "3000:3000")]
        [InlineData("docker run -p 80:80 -p 443:443 nginx", "80:80")]
        public async Task ConvertCommand_WithDifferentPortFormats_ParsesCorrectly(string input, string expectedPort)
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = input;

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains($"- \"{expectedPort}\"", output);
        }

        [Theory]
        [InlineData("docker run -v /host:/container nginx", "/host:/container")]
        [InlineData("docker run --volume /host:/container:ro nginx", "/host:/container:ro")]
        public async Task ConvertCommand_WithDifferentVolumeFormats_ParsesCorrectly(string input, string expectedVolume)
        {
            // Arrange
            var viewModel = new DockerComposeConverterViewModel();
            viewModel.DockerRunInput = input;

            // Act
            await viewModel.ConvertCommand();

            // Assert
            var output = viewModel.DockerComposeOutput;
            Assert.Contains($"- {expectedVolume}", output);
        }
    }
}