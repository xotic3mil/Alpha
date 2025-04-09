namespace Business.Dtos;

public class ProjectManagementResult : StatusResults
{

}

public class ProjectManagementResult<T> : StatusResults
{
    public T? Result { get; set; }
}
