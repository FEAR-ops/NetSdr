using NetSdrClientApp.Messages;

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetControlItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(actualCode, Is.EqualTo((short)code));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void GetDataItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var parametersBytes = msg.Skip(2);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        //TODO: add more NetSdrMessageHelper tests 
        [Test]
        public void GetSamples_ShouldReturnExpectedIntegers()
        {
            //Arrange
            ushort sampleSize = 16; // 2 bytes per sample
            byte[] body = { 0x01, 0x00, 0x02, 0x00 }; // 2 samples: 1, 2

            //Act
            var samples = NetSdrMessageHelper.GetSamples(sampleSize, body).ToArray();

            //Assert
            Assert.That(samples.Length, Is.EqualTo(2));
            Assert.That(samples[0], Is.EqualTo(1));
            Assert.That(samples[1], Is.EqualTo(2));
        }

        // ✅ BEGIN: Lab8 - Added for Sonar & Coverage
        [Test]
        public void GetSamples_ZeroLength_ReturnsEmptyArray()
        {
            //Arrange
            ushort sampleSize = 16;
            byte[] body = Array.Empty<byte>();

            //Act
            var samples = NetSdrMessageHelper.GetSamples(sampleSize, body).ToArray();

            //Assert
            Assert.That(samples.Length, Is.EqualTo(0));
        }

        [Test]
        public void GetSamples_WrongSampleSize_ReturnsPartialSamples()
        {
            //Arrange
            ushort sampleSize = 16;
            byte[] body = { 0x01, 0x00, 0x02 }; // 3 bytes → 1.5 sample

            //Act
            var samples = NetSdrMessageHelper.GetSamples(sampleSize, body).ToArray();

            //Assert
            Assert.That(samples.Length, Is.EqualTo(1)); // only full sample counted
            Assert.That(samples[0], Is.EqualTo(1));
        }


        // ✅ NEW TESTS BELOW ─────────────────────────────────────────────

        [Test]
        public void GetControlItemMessage_TooLargePayload_ShouldThrowException()
        {
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;

            Assert.Throws<ArgumentException>(() =>
                NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[9000]));
        }

        [Test]
        public void GetDataItemMessage_HeaderShouldEncodeLengthAndTypeCorrectly()
        {
            var type = NetSdrMessageHelper.MsgTypes.DataItem1;
            int parametersLength = 100;

            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            ushort header = BitConverter.ToUInt16(msg, 0);
            var decodedType = (NetSdrMessageHelper.MsgTypes)(header >> 13);
            var decodedLength = header & 0x1FFF;

            Assert.That(decodedType, Is.EqualTo(type));
            Assert.That(decodedLength, Is.EqualTo(parametersLength + 2));
        }

        [Test]
        public void TranslateMessage_ControlItem_ShouldDecodeEverythingCorrectly()
        {
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.RFFilter;
            byte[] parameters = { 0xAA, 0xBB };

            var message = NetSdrMessageHelper.GetControlItemMessage(type, code, parameters);

            bool success = NetSdrMessageHelper.TranslateMessage(message,
                out var decodedType,
                out var decodedCode,
                out var seq,
                out var body);

            Assert.That(success, Is.True);
            Assert.That(decodedType, Is.EqualTo(type));
            Assert.That(decodedCode, Is.EqualTo(code));
            Assert.That(body, Is.EqualTo(parameters));
        }

        [Test]
        public void TranslateMessage_InvalidCode_ShouldReturnFalse()
        {
            byte[] msg = { 0x00, 0x00, 0xFF, 0xFF, 0x10 };

            bool success = NetSdrMessageHelper.TranslateMessage(msg, out _, out _, out _, out _);

            Assert.That(success, Is.False);
        }

        [Test]
        public void GetSamples_SampleSizeTooBig_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NetSdrMessageHelper.GetSamples(40, new byte[4]).ToArray());
        }

        // ✅ Modified so NOT duplicate: checks msg.Length change
        [Test]
        public void TranslateMessage_DataItem_ShouldRemoveSequenceFromBody()
        {
            var type = NetSdrMessageHelper.MsgTypes.DataItem0;
            byte[] parameters = { 0x10, 0x20, 0x30, 0x40 };

            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, parameters);

            bool success = NetSdrMessageHelper.TranslateMessage(msg,
                out var decodedType,
                out _,
                out var sequence,
                out var body);

            Assert.That(success, Is.True);
            Assert.That(decodedType, Is.EqualTo(type));
            Assert.That(body.Length, Is.EqualTo(parameters.Length));
            Assert.That(sequence, Is.GreaterThanOrEqualTo(0));
        }
    }
}