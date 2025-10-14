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
        // ? ����� ���� 1 � �������� �� null ��� ���������
        [Test]
        public void GetControlItemMessage_ShouldThrow_WhenParametersNull()
        {
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;

            Assert.Throws<ArgumentNullException>(() =>
                NetSdrMessageHelper.GetControlItemMessage(type, code, null));
        }

        // ? ����� ���� 2 � �������� �� ������� ������� ���������
        [Test]
        public void GetControlItemMessage_ShouldWork_WithZeroLengthParameters()
        {
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;

            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, Array.Empty<byte>());

            Assert.That(msg.Length, Is.GreaterThanOrEqualTo(4)); // ����� ��������� + ���
        }

        // ? ����� ���� 3 � ����� ��� ����������� ��� DataItem
        [Test]
        public void GetDataItemMessage_OtherType_WorksCorrectly()
        {
            var type = NetSdrMessageHelper.MsgTypes.DataItem1;
            int parametersLength = 100;

            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var num = BitConverter.ToUInt16(msg.Take(2).ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            Assert.That(actualType, Is.EqualTo(type));
        }

        // ? ����� ���� 4 � ����������� ��� ��� ��� (���� ����� ������� �� ���������)
        [Test]
        public void GetControlItemMessage_InvalidCode_ShouldNotCrash()
        {
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var invalidCode = (NetSdrMessageHelper.ControlItemCodes)9999;

            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, invalidCode, new byte[10]);
            Assert.That(msg.Length, Is.GreaterThan(0));
        }
    }
}