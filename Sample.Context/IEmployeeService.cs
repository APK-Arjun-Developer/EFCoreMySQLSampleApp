using Sample.Model;

namespace Sample.Context
{
    public interface IEmployeeService
    {
        Task<List<EmployeeEntity>> GetEmployees();
        Task<EmployeeEntity?> GetEmployee(int id);
        Task<bool> CreateEmployee(EmployeeEntity employee);
        Task<bool> UpdateEmployee(EmployeeEntity employee);
        Task<bool> DeleteEmployee(int id);
    }
}