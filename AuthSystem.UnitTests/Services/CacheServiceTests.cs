using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using AuthSystem.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AuthSystem.UnitTests.Services
{
    public class CacheServiceTests
    {
        private readonly Mock<ILogger<MemoryCacheService>> _loggerMock;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;
        private readonly ICacheService _cacheService;

        public CacheServiceTests()
        {
            _loggerMock = new Mock<ILogger<MemoryCacheService>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _cacheSettings = new CacheSettings
            {
                Provider = "Memory",
                DefaultAbsoluteExpirationMinutes = 60,
                DefaultSlidingExpirationMinutes = 20,
                LdapCacheAbsoluteExpirationMinutes = 120,
                ConfigurationCacheAbsoluteExpirationMinutes = 240,
                UserCacheAbsoluteExpirationMinutes = 30,
                RoleCacheAbsoluteExpirationMinutes = 60,
                PermissionCacheAbsoluteExpirationMinutes = 60
            };

            _cacheService = new MemoryCacheService(_memoryCache, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            string key = "non_existent_key";

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_ShouldStoreValueInCache()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";

            // Act
            var setResult = await _cacheService.SetAsync(key, value);
            var getResult = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.True(setResult);
            Assert.Equal(value, getResult);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveValueFromCache()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            await _cacheService.SetAsync(key, value);

            // Act
            var removeResult = await _cacheService.RemoveAsync(key);
            var getResult = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.True(removeResult);
            Assert.Null(getResult);
        }

        [Fact]
        public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            await _cacheService.SetAsync(key, value);

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            string key = "non_existent_key";

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetOrSetAsync_WhenKeyDoesNotExist_ShouldCallFactoryAndStoreValue()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            bool factoryCalled = false;

            // Act
            var result = await _cacheService.GetOrSetAsync(key, async () =>
            {
                factoryCalled = true;
                return value;
            });

            // Assert
            Assert.True(factoryCalled);
            Assert.Equal(value, result);
            Assert.Equal(value, await _cacheService.GetAsync<string>(key));
        }

        [Fact]
        public async Task GetOrSetAsync_WhenKeyExists_ShouldNotCallFactory()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            await _cacheService.SetAsync(key, value);
            bool factoryCalled = false;

            // Act
            var result = await _cacheService.GetOrSetAsync(key, async () =>
            {
                factoryCalled = true;
                return "new_value";
            });

            // Assert
            Assert.False(factoryCalled);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task IncrementAsync_ShouldIncrementValue()
        {
            // Arrange
            string key = "counter_key";
            await _cacheService.SetAsync(key, 5L);

            // Act
            var result = await _cacheService.IncrementAsync(key, 3);

            // Assert
            Assert.Equal(8L, result);
            Assert.Equal(8L, await _cacheService.GetAsync<long>(key));
        }

        [Fact]
        public async Task DecrementAsync_ShouldDecrementValue()
        {
            // Arrange
            string key = "counter_key";
            await _cacheService.SetAsync(key, 5L);

            // Act
            var result = await _cacheService.DecrementAsync(key, 2);

            // Assert
            Assert.Equal(3L, result);
            Assert.Equal(3L, await _cacheService.GetAsync<long>(key));
        }

        [Fact]
        public async Task RefreshExpirationAsync_WhenKeyExists_ShouldReturnTrue()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            await _cacheService.SetAsync(key, value);

            // Act
            var result = await _cacheService.RefreshExpirationAsync(key);

            // Assert
            Assert.True(result);
            Assert.Equal(value, await _cacheService.GetAsync<string>(key));
        }

        [Fact]
        public async Task RefreshExpirationAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            string key = "non_existent_key";

            // Act
            var result = await _cacheService.RefreshExpirationAsync(key);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveByPatternAsync_ShouldRemoveMatchingKeys()
        {
            // Arrange
            await _cacheService.SetAsync("user:1", "value1");
            await _cacheService.SetAsync("user:2", "value2");
            await _cacheService.SetAsync("role:1", "value3");

            // Act
            var result = await _cacheService.RemoveByPatternAsync("user:*");

            // Assert
            Assert.Equal(2, result);
            Assert.Null(await _cacheService.GetAsync<string>("user:1"));
            Assert.Null(await _cacheService.GetAsync<string>("user:2"));
            Assert.NotNull(await _cacheService.GetAsync<string>("role:1"));
        }

        [Fact]
        public async Task ClearAsync_ShouldRemoveAllKeys()
        {
            // Arrange
            await _cacheService.SetAsync("key1", "value1");
            await _cacheService.SetAsync("key2", "value2");

            // Act
            var result = await _cacheService.ClearAsync();

            // Assert
            Assert.True(result);
            Assert.Null(await _cacheService.GetAsync<string>("key1"));
            Assert.Null(await _cacheService.GetAsync<string>("key2"));
        }

        [Fact]
        public async Task GetTimeToLiveAsync_WhenKeyExists_ShouldReturnNonNegativeValue()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            await _cacheService.SetAsync(key, value);

            // Act
            var result = await _cacheService.GetTimeToLiveAsync(key);

            // Assert
            Assert.True(result >= -1); // -1 indica que la clave existe pero no tiene tiempo de expiración
        }

        [Fact]
        public async Task GetTimeToLiveAsync_WhenKeyDoesNotExist_ShouldReturnNegativeTwo()
        {
            // Arrange
            string key = "non_existent_key";

            // Act
            var result = await _cacheService.GetTimeToLiveAsync(key);

            // Assert
            Assert.Equal(-2, result); // -2 indica que la clave no existe
        }
    }
}
