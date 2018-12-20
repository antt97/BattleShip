
Create Table Users
(
	UserName nvarchar(128) primary key,
	Stage int default 0,
	Position0 int default 0,
	Position1 int default 0,
	RoomId bigint default 0
)
Update Users set Position1 = 0;
Create Table [Rooms]
(
	RoomId bigint IDENTITY(10001,1) primary key,
	DateCreate datetime default getdate(),
	DateUpdate datetime default getdate(),
	UserName nvarchar(128),
	Status int default 1
)

Create table Members
(
	MemberId bigint IDENTITY(10001,1) primary key,
	RoomId bigint,
	UserName nvarchar(128),
	Matrix nvarchar(MAX),
	IsTurn bit default 0,
	Result int default 0
)


Create procedure InsertUser(@UserName nvarchar(128))
as
	Insert into Users(UserName) values(@UserName);
go
Create procedure GetUser(@UserName nvarchar(128))
as
	Select * from Users where UserName = @UserName;
go
Alter procedure UpdateUser
(
	@UserName nvarchar(128),
	@Stage int,
	@Position0 int,
	@Position1 int,
	@RoomId bigint
)
as
	Update Users set Stage = @Stage, Position0 = @Position0, Position1 = @Position1, RoomId = @RoomId where UserName = @UserName;
go

Alter procedure InsertRoom
(
	@RoomId bigint output,
	@UserName nvarchar(128)
)
as
	Insert into Rooms(UserName) values(@UserName)
	set @RoomId=SCOPE_IDENTITY();
go
Alter procedure GetRoomsPending
as
	Select * from Rooms where  Status = 1 or Status = 2;
go
Create procedure GetRoomById(@RoomId bigint)
as
	Select * from Rooms where RoomId = @RoomId;
go
Create procedure UpdateRoom(@RoomId bigint, @Status int)
as
	Update Rooms set Status = @Status where RoomId = @RoomId;
go


Alter procedure InsertMember
(
	@MemberId bigint output,
	@RoomId bigint,
	@UserName nvarchar(128)
)
as
	Insert into Members(RoomId, UserName) values(@RoomId, @Username);
	set @MemberId = SCOPE_IDENTITY();
	Update Rooms set DateUpdate = getdate() where RoomId = @RoomId;
go
Create procedure GetMembersByRoomId(@RoomId bigint)
as
	Select * from Members where RoomId = @RoomId;
go
Create procedure GetMemberByRoomId(@RoomId bigint, @UserName nvarchar(128))
as
	Select * from Members where RoomId = @RoomId and UserName = @UserName;
go

Alter procedure UpdateMember
(
	@MemberId bigint,
	@RoomId bigint,
	@Matrix nvarchar(MAX),
	@IsTurn bit,
	@Result int
)
as
	Update Members set Matrix = @Matrix, IsTurn = @IsTurn, Result = @Result where MemberId = @MemberId;
	Update Rooms set DateUpdate = getdate() where RoomId = @RoomId;
go
