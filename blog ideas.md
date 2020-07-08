- xUnit with generic input parameters

        [Theory]
        [InlineData("06-00-01-02-06-00", HubProperty.Button, HubPropertyOperation.Update, false)]
        [InlineData("06-00-01-02-06-01", HubProperty.Button, HubPropertyOperation.Update, true)]
        public void HubPropertiesEncoder_Decode<T>(string messageAsString, HubProperty expectedProperty, HubPropertyOperation expectedPropertyOperation, T payload)
        {
            // arrange
            var data = StringToData(messageAsString).AsSpan().Slice(3);

            // act
            var message = HubPropertiesEncoder.Decode(data) as HubPropertyMessage<T>;

            // assert
            Assert.Equal(expectedProperty, message.Property);
            Assert.Equal(expectedPropertyOperation, message.Operation);
            Assert.Equal(payload, message.Payload);
        }

- using middlewares vs. System.Reactive

- using comm traces instead of setter routines for intializing static properties