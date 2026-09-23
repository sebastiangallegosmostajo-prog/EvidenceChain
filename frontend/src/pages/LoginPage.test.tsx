import {
  render,
  screen,
} from '@testing-library/react'
import userEvent from
  '@testing-library/user-event'
import {
  beforeEach,
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import {
  ApiError,
} from '../services/api'
import {
  login,
} from '../services/authService'
import {
  LoginPage,
} from './LoginPage'

vi.mock(
  '../services/authService',
  () => ({
    login: vi.fn(),
  }),
)

const loginMock =
  vi.mocked(login)

describe('LoginPage', () => {
  beforeEach(() => {
    loginMock.mockReset()
  })

  it(
    'inicia sesión con credenciales válidas',
    async () => {
      const user =
        userEvent.setup()

      const onLogin =
        vi.fn()

      loginMock.mockResolvedValue({
        userId:
          '11111111-1111-1111-1111-111111111111',
        name:
          'Investigador Demo',
        email:
          'investigador@evidencechain.local',
        role:
          'Investigator',
      })

      render(
        <LoginPage
          onLogin={onLogin}
        />,
      )

      await user.type(
        screen.getByLabelText(
          /correo electrónico/i,
        ),
        'investigador@evidencechain.local',
      )

      await user.type(
        screen.getByLabelText(
          /contraseña/i,
        ),
        'TestPassword123!',
      )

      await user.click(
        screen.getByRole(
          'button',
          {
            name: /ingresar/i,
          },
        ),
      )

      expect(loginMock)
        .toHaveBeenCalledWith({
          email:
            'investigador@evidencechain.local',
          password:
            'TestPassword123!',
        })

      expect(onLogin)
        .toHaveBeenCalledWith(
          expect.objectContaining({
            role: 'Investigator',
          }),
        )
    },
  )

  it(
    'muestra el error de credenciales inválidas',
    async () => {
      const user =
        userEvent.setup()

      loginMock.mockRejectedValue(
        new ApiError(
          'El correo o la contraseña son incorrectos.',
          401,
        ),
      )

      render(
        <LoginPage
          onLogin={vi.fn()}
        />,
      )

      await user.type(
        screen.getByLabelText(
          /correo electrónico/i,
        ),
        'incorrecto@evidencechain.local',
      )

      await user.type(
        screen.getByLabelText(
          /contraseña/i,
        ),
        'incorrecta',
      )

      await user.click(
        screen.getByRole(
          'button',
          {
            name: /ingresar/i,
          },
        ),
      )

      expect(
        await screen.findByRole(
          'alert',
        ),
      ).toHaveTextContent(
        'El correo o la contraseña son incorrectos.',
      )
    },
  )
})