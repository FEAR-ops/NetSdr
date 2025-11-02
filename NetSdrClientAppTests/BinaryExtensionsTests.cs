using NetSdrClientApp.Messages;

namespace NetSdrClientAppTests
{
    public class BinaryExtensionsTests
    {
        // ✅ BEGIN: Lab8 - Added for Sonar & Coverage

        [Test]
        public void ToUInt16_ShouldConvertTwoBytesLittleEndian()
        {
            byte[] data = { 0x01, 0x00 };
            ushort result = data.ToUInt16();
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ToInt16_ShouldConvertTwoBytesLittleEndian()
        {
            byte[] data = { 0xFF, 0x7F }; // 32767
            short result = data.ToInt16();
            Assert.That(result, Is.EqualTo(32767));
        }

        [Test]
        public void ToUInt32_ShouldConvertFourBytesLittleEndian()
        {
            byte[] data = { 0x01, 0x00, 0x00, 0x00 };
            uint result = data.ToUInt32();
            Assert.That(result, Is.EqualTo(1u));
        }

        [Test]
        public void ToInt32_ShouldConvertFourBytesLittleEndian()
        {
            byte[] data = { 0xFF, 0xFF, 0xFF, 0x7F }; // 2147483647
            int result = data.ToInt32();
            Assert.That(result, Is.EqualTo(2147483647));
        }

        [Test]
        public void ToSingle_ShouldConvertFloatValue()
        {
            float value = 3.14f;
            byte[] data = BitConverter.GetBytes(value);

            float result = data.ToSingle();

            Assert.That(result, Is.EqualTo(value).Within(0.0001));
        }

        [Test]
        public void ToDouble_ShouldConvertDoubleValue()
        {
            double value = 3.14159265359;
            byte[] data = BitConverter.GetBytes(value);

            double result = data.ToDouble();

            Assert.That(result, Is.EqualTo(value).Within(1e-10));
        }

        // ✅ END: Lab8
    }
}
