using System;
using System.Collections.Generic;
using System.Linq;
using Identidade.Publico.Enumerations;

namespace Identidade.Infraestrutura.Helpers
{
    public interface IPermissionOperationManager
    {
        string[] GetOperations(int operationSum);
        int GetOperationSum(IEnumerable<string> operations);
    }

    internal class PermissionOperationManager : IPermissionOperationManager
    {
        public string[] GetOperations(int operationSum)
        {
            operationSum = Math.Min(operationSum, (int) PermissionOperation.All);
            var operations = new List<string>();

            var operationInts = Enum.GetValues(typeof(PermissionOperation))
                .Cast<int>()
                .OrderByDescending(i => i);

            foreach (int operation in operationInts)
            {
                if (operationSum - operation < 0)
                    continue;

                operations.Add(((PermissionOperation)operation).ToString());
                operationSum -= operation;
            }

            return operations.ToArray();
        }

        public int GetOperationSum(IEnumerable<string> operations) =>
            Math.Min(
                operations.Sum(operation => (int)Enum.Parse(typeof(PermissionOperation), operation)),
                (int) PermissionOperation.All);
    }
}
