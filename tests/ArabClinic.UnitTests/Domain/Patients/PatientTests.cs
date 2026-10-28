using ArabClinic.Domain.Patients;

namespace ArabClinic.UnitTests.Domain.Patients;

public sealed class PatientTests
{
    [Fact]
    public void ConstructorWithValidValuesCreatesPatient()
    {
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var patient = new Patient("  Ahmed Ali  ", dateOfBirth);

        Assert.NotEqual(Guid.Empty, patient.Id);
        Assert.Equal("Ahmed Ali", patient.FullName);
        Assert.Equal(dateOfBirth, patient.DateOfBirth);
    }

    [Fact]
    public void ConstructorWithEmptyNameThrows()
    {
        Assert.Throws<ArgumentException>(() =>
            new Patient(" ", new DateOnly(1990, 1, 1)));
    }
}