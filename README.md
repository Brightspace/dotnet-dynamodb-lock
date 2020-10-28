# dotnet-dynamodb-lock

Dotnet tool for acquiring/releasing locks in a DynamoDB table.

## Usage

### Acquire

```
dotnet dynamodb-lock acquire --config <configFile> --label <label> --output <tokenFile>
```

The label is used to provide context to others trying to acquire the lock

```
> dotnet dynamodb-lock acquire --config lock.config --label 'Build 123' --output token.txt

Locked by 'Build 122' at 8:50:43 PM. Expires at 9:20:43 PM.
```

### Release

```
dotnet dynamodb-lock release --config <configFile> --token <tokenFile>
```

The token file is the output of the acquire command.

```
> dotnet dynamodb-lock release --config lock.config --token token.txt

Lock released.
```

### Config

```
{
	"tableName": "locks",
	"lockKey": "build",

	"awsRegion": "us-east-1",
	"roleArn": "arn:aws:iam::111111111111:role/build",

	"acquireTimeout":300,
	"lockDuration":1800,
	"retryAfter":5
}
```

- **tableName**: The DynamoDB table name `[required]`

- **lockKey**: The unique key for the lock `[required]`

- **awsRegion**: The aws region of the table `[optional]`

- **roleArn**: The arn of the role to assume `[optional]`

- **acquireTimeout**: How long to wait to acquire the lock before timing out `[optional]`

- **lockDuration**: How long the lock is valid for `[optional]`

- **retryAfter**: How long to wait inbetween retries to acquire the lock `[optional]`


## Schena

- **Simple Primary Key**:

	```
	key : string
	```

- **Time To Live**:

	```
	expires : numeric
	```

## Permissions

- dynamodb:PutItem
- dynamodb:GetItem
- dynamodb:DeleteItem
