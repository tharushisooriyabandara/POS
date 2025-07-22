package mock

// import (
// 	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
// 	"github.com/Delivergate-Dev/pos-service-golang/service"
// )

// type UserServiceMock struct {
// 	// Mock functions
// 	GetUsersFn func() ([]db.User, error)

// 	// Call counters for verification
// 	GetUsersCallCount int
// }

// // NewUserServiceMock creates a new mock instance
// func NewUserServiceMock() *UserServiceMock {
// 	return &UserServiceMock{
// 		// Default implementation returns empty slice
// 		GetUsersFn: func() ([]db.User, error) {
// 			return []db.User{}, nil
// 		},
// 	}
// }

// // Verify this implements the interface
// var _ service.IUserService = (*UserServiceMock)(nil)

// // GetUsers implements service.IUserService
// func (m *UserServiceMock) GetUsers() ([]db.User, error) {
// 	m.GetUsersCallCount++
// 	return m.GetUsersFn()
// }

// // Reset resets all function implementations and call counts
// func (m *UserServiceMock) Reset() {
// 	m.GetUsersFn = func() ([]db.User, error) {
// 		return []db.User{}, nil
// 	}
// 	m.GetUsersCallCount = 0
// }

// // AssertGetUsersCalled asserts that GetUsers was called exactly n times
// func (m *UserServiceMock) AssertGetUsersCalled(n int) bool {
// 	return m.GetUsersCallCount == n
// }
